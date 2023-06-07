﻿using Binance.Net.Enums;

using MarinerX.Bot.Clients;
using MarinerX.Bot.Models;

using MercuryTradingModel.Maths;

using System;
using System.Reflection;
using System.Threading.Tasks;

namespace MarinerX.Bot.Bots
{
    public class LongBot : Bot
    {
        public decimal BaseOrderSize { get; set; }
        public decimal TargetRoe { get; set; }
        public decimal Leverage { get; set; }

        public LongBot() : this("", "")
        {

        }

        public LongBot(string name) : this(name, "")
        {

        }

        public LongBot(string name, string description)
        {
            Name = name;
            Description = description;
        }

        public async Task Evaluate()
        {
            try
            {
                foreach (var pairQuote in Account.PairQuotes)
                {
                    var c0 = pairQuote.Charts[^1]; // 현재 정보

                    if (Account.IsPositioning(pairQuote.Symbol, PositionSide.Long)) // 포지션이 있으면
                    {
                        var position = Account.GetPosition(pairQuote.Symbol, PositionSide.Long);
                        if (position == null)
                        {
                            return;
                        }
                        var roe = position.Roe;
                        var quantity = position.Quantity;

                        // 목표 수익률의 절반만큼 손실일 경우 현재가 전량 손절
                        if (roe <= TargetRoe / -2)
                        {
                            var price = c0.Quote.Close;
                            await CloseSell(pairQuote.Symbol, price, quantity).ConfigureAwait(false);
                        }
                        // 목표 수익률에 도달하면 익절
                        else if (roe >= TargetRoe)
                        {
                            var price = c0.Quote.Close;
                            await CloseSell(pairQuote.Symbol, price, quantity).ConfigureAwait(false);
                        }
                    }
                    else // 포지션이 없으면
                    {
                        var c1 = pairQuote.Charts[^2]; // 1봉전 정보
                        var c2 = pairQuote.Charts[^3]; // 2봉전 정보
                        var c3 = pairQuote.Charts[^4]; // 3봉전 정보
                        var c4 = pairQuote.Charts[^5]; // 4봉전 정보

                        // RSI 40라인을 골든 크로스 이후, 3봉 이내에 LSMA 10이 30을 골든 크로스하면 매수
                        if (c0.Lsma10 > c0.Lsma30 && c1.Lsma10 < c1.Lsma30 &&
                            ((c0.Rsi > 40 && c1.Rsi < 40) || (c1.Rsi > 40 && c2.Rsi < 40) || (c2.Rsi > 40 && c3.Rsi < 40) || (c3.Rsi > 40 && c4.Rsi < 40)))
                        {
                            var price = c0.Quote.Close;
                            var quantity = BaseOrderSize / price;
                            await OpenBuy(pairQuote.Symbol, price, quantity).ConfigureAwait(false);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(nameof(LongBot), MethodBase.GetCurrentMethod()?.Name, ex);
            }
        }

        public async Task MockEvaluate()
        {
            try
            {
                foreach (var pairQuote in Account.PairQuotes)
                {
                    var c0 = pairQuote.Charts[^1]; // 현재 정보

                    if (Account.IsMockPositioning(pairQuote.Symbol, PositionSide.Long)) // 포지션이 있으면
                    {
                        var position = Account.GetMockPosition(pairQuote.Symbol, PositionSide.Long);
                        if (position == null)
                        {
                            return;
                        }
                        position.MarkPrice = pairQuote.CurrentPrice; // Mock 전용
                        var roe = position.Roe;
                        var quantity = position.Quantity;

                        // 목표 수익률의 절반만큼 손실일 경우 현재가 전량 손절
                        if (roe <= TargetRoe / -2)
                        {
                            var price = c0.Quote.Close;
                            MockCloseSell(pairQuote.Symbol, price, quantity);
                        }
                        // 목표 수익률에 도달하면 익절
                        else if (roe >= TargetRoe)
                        {
                            var price = c0.Quote.Close;
                            MockCloseSell(pairQuote.Symbol, price, quantity);
                        }
                    }
                    else // 포지션이 없으면
                    {
                        var c1 = pairQuote.Charts[^2]; // 1봉전 정보
                        var c2 = pairQuote.Charts[^3]; // 2봉전 정보
                        var c3 = pairQuote.Charts[^4]; // 3봉전 정보
                        var c4 = pairQuote.Charts[^5]; // 4봉전 정보

                        // RSI 40라인을 골든 크로스 이후, 3봉 이내에 LSMA 10이 30을 골든 크로스하면 매수
                        if (c0.Lsma10 > c0.Lsma30 && c1.Lsma10 < c1.Lsma30 &&
                            ((c0.Rsi > 40 && c1.Rsi < 40) || (c1.Rsi > 40 && c2.Rsi < 40) || (c2.Rsi > 40 && c3.Rsi < 40) || (c3.Rsi > 40 && c4.Rsi < 40)))
                        {
                            var price = c0.Quote.Close;
                            var quantity = BaseOrderSize / price;
                            MockOpenBuy(pairQuote.Symbol, price, quantity);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(nameof(LongBot), MethodBase.GetCurrentMethod()?.Name, ex);
            }
        }

        public async Task MonitorOpenOrderTimeout()
        {
            try
            {
                var openOrderResult = BinanceClients.Api.UsdFuturesApi.Trading.GetOpenOrdersAsync();
                openOrderResult.Wait();
                foreach (var order in openOrderResult.Result.Data)
                {
                    if ((DateTime.UtcNow - order.CreateTime) >= TimeSpan.FromMinutes(5)) // 5분이 넘도록 체결이 안되면 주문 취소
                    {
                        await BinanceClients.Api.UsdFuturesApi.Trading.CancelOrderAsync(order.Symbol, order.Id).ConfigureAwait(false);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(nameof(LongBot), MethodBase.GetCurrentMethod()?.Name, ex);
            }
        }

        public async Task OpenBuy(string symbol, decimal price, decimal quantity)
        {
            try
            {
                var limitPrice = StockUtil.GetPriceByRoe(MercuryTradingModel.Enums.PositionSide.Long, price, 0.25m); // +0 ~ +0.25%
                var result = await BinanceClients.Api.UsdFuturesApi.Trading.PlaceOrderAsync(symbol, OrderSide.Buy, FuturesOrderType.Limit, quantity, limitPrice, PositionSide.Long, TimeInForce.GoodTillCanceled).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Logger.Log(nameof(LongBot), MethodBase.GetCurrentMethod()?.Name, ex);
            }
        }

        public async Task CloseSell(string symbol, decimal price, decimal quantity)
        {
            try
            {
                var limitPrice = StockUtil.GetPriceByRoe(MercuryTradingModel.Enums.PositionSide.Long, price, 1m); // -0 ~ -1%
                var result = await BinanceClients.Api.UsdFuturesApi.Trading.PlaceOrderAsync(symbol, OrderSide.Sell, FuturesOrderType.Limit, quantity, limitPrice, PositionSide.Long, TimeInForce.GoodTillCanceled).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Logger.Log(nameof(LongBot), MethodBase.GetCurrentMethod()?.Name, ex);
            }
        }

        public void MockOpenBuy(string symbol, decimal price, decimal quantity)
        {
            try
            {
                Account.Positions.Add(new BinancePosition(symbol, "Long", 0, price, price, quantity, Leverage));
                Account.AddHistory($"Open Buy {symbol}, {price}, {quantity}");
            }
            catch (Exception ex)
            {
                Logger.Log(nameof(LongBot), MethodBase.GetCurrentMethod()?.Name, ex);
            }
        }

        public void MockCloseSell(string symbol, decimal price, decimal quantity)
        {
            try
            {
                var position = Account.Positions.Find(a => a.Symbol.Equals(symbol));
                Account.Positions.Remove(position);
                Account.AddHistory($"Close Sell {symbol}, {price}, {quantity}");
            }
            catch (Exception ex)
            {
                Logger.Log(nameof(LongBot), MethodBase.GetCurrentMethod()?.Name, ex);
            }
        }
    }
}
