using Binance.Net.Enums;

using MarinerX.Bot.Clients;
using MarinerX.Bot.Extensions;
using MarinerX.Bot.Models;

using MercuryTradingModel.Maths;

using System;
using System.Reflection;
using System.Threading.Tasks;

namespace MarinerX.Bot.Bots
{
    public class ShortBot : Bot
    {
        public decimal BaseOrderSize { get; set; }
        public decimal TargetRoe { get; set; }
        public int Leverage { get; set; }
        public int MaxActiveDeals { get; set; }

        public ShortBot() : this("", "")
        {

        }

        public ShortBot(string name) : this(name, "")
        {

        }

        public ShortBot(string name, string description)
        {
            Name = name;
            Description = description;
        }

        public async Task Evaluate()
        {
            try
            {
                if (Common.ShortPositions.Count >= MaxActiveDeals) // 동시 거래 수 MAX
                {
                    return;
                }

                foreach (var pairQuote in Common.PairQuotes)
                {
                    var symbol = pairQuote.Symbol;
                    if (!Common.IsShortPositioning(symbol)) // 포지션이 없으면
                    {
                        var c0 = pairQuote.Charts[^1]; // 현재 정보
                        var c1 = pairQuote.Charts[^2]; // 1봉전 정보
                        var c2 = pairQuote.Charts[^3]; // 2봉전 정보
                        var c3 = pairQuote.Charts[^4]; // 3봉전 정보
                        var c4 = pairQuote.Charts[^5]; // 4봉전 정보

                        // 기법 :: RSI 60라인을 데드 크로스 이후, 3봉 이내에 LSMA 10이 30을 데드 크로스하면 진입
                        if (c0.Lsma10 < c0.Lsma30 && c1.Lsma10 > c1.Lsma30 &&
                            ((c0.Rsi < 60 && c1.Rsi > 60) || (c1.Rsi < 60 && c2.Rsi > 60) || (c2.Rsi < 60 && c3.Rsi > 60)))
                        {
                            var price = c0.Quote.Close;
                            var quantity = (BaseOrderSize / price).ToValidQuantity(symbol);
                            await OpenSell(symbol, price, quantity).ConfigureAwait(false);
                            var takeProfitPrice = StockUtil.GetPriceByRoe(MercuryTradingModel.Enums.PositionSide.Short, price, TargetRoe);
                            await SetTakeProfit(symbol, takeProfitPrice, quantity).ConfigureAwait(false);
                            var stoplossPrice = StockUtil.GetPriceByRoe(MercuryTradingModel.Enums.PositionSide.Short, price, TargetRoe / -2);
                            await SetStopLoss(symbol, stoplossPrice, quantity).ConfigureAwait(false);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(nameof(ShortBot), MethodBase.GetCurrentMethod()?.Name, ex);
            }
        }

        public async Task MockEvaluate()
        {
            try
            {
                foreach (var pairQuote in Common.PairQuotes)
                {
                    var c0 = pairQuote.Charts[^1]; // 현재 정보

                    if (Common.IsMockPositioning(pairQuote.Symbol, PositionSide.Long)) // 포지션이 있으면
                    {
                        var position = Common.GetMockPosition(pairQuote.Symbol, PositionSide.Long);
                        if (position == null)
                        {
                            continue;
                        }
                        position.MarkPrice = pairQuote.CurrentPrice; // Mock 전용
                        position.Pnl = (position.MarkPrice - position.EntryPrice) * position.Quantity;
                        var roe = position.Roe;
                        var quantity = position.Quantity;

                        // 목표 수익률의 절반만큼 손실일 경우 현재가 전량 손절
                        if (roe <= TargetRoe * Leverage / -2)
                        {
                            var price = c0.Quote.Close;
                            MockCloseSell(pairQuote.Symbol, price, quantity);
                        }
                        // 목표 수익률에 도달하면 익절
                        else if (roe >= TargetRoe * Leverage)
                        {
                            var price = c0.Quote.Close;
                            MockCloseSell(pairQuote.Symbol, price, quantity);
                        }
                    }
                    else // 포지션이 없으면
                    {
                        if (Common.LongMockPositions.Count >= MaxActiveDeals) // 동시 거래 수 MAX
                        {
                            continue;
                        }

                        var c1 = pairQuote.Charts[^2]; // 1봉전 정보
                        var c2 = pairQuote.Charts[^3]; // 2봉전 정보
                        var c3 = pairQuote.Charts[^4]; // 3봉전 정보
                        var c4 = pairQuote.Charts[^5]; // 4봉전 정보

                        // RSI 40라인을 골든 크로스 이후, 3봉 이내에 LSMA 10이 30을 골든 크로스하면 매수
                        if (c0.Lsma10 > c0.Lsma30 && c1.Lsma10 < c1.Lsma30 &&
                            ((c0.Rsi > 40 && c1.Rsi < 40) || (c1.Rsi > 40 && c2.Rsi < 40) || (c2.Rsi > 40 && c3.Rsi < 40)))
                        {
                            var price = c0.Quote.Close;
                            var quantity = Math.Round(BaseOrderSize / price, 4);
                            MockOpenBuy(pairQuote.Symbol, price, quantity);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(nameof(ShortBot), MethodBase.GetCurrentMethod()?.Name, ex);
            }
        }

        /// <summary>
        /// 아직은 사용하지 않음
        /// </summary>
        /// <returns></returns>
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
                Logger.Log(nameof(ShortBot), MethodBase.GetCurrentMethod()?.Name, ex);
            }
        }

        public async Task OpenSell(string symbol, decimal price, decimal quantity)
        {
            try
            {
                await BinanceClients.Api.UsdFuturesApi.Account.ChangeInitialLeverageAsync(symbol, Leverage); // 레버리지 설정
                var limitPrice = StockUtil.GetPriceByRoe(MercuryTradingModel.Enums.PositionSide.Long, price, -0.25m).ToValidPrice(symbol); // -0 ~ -0.25%

                var result = await BinanceClients.OpenSell(symbol, limitPrice, quantity).ConfigureAwait(false);
                if (result.Success)
                {
                    Common.AddHistory($"Open Sell {symbol}, {price}, {quantity}");
                }
                else
                {
                    Common.AddHistory($"Open Sell Error: {result.Error?.Message}");
                }
            }
            catch (Exception ex)
            {
                Logger.Log(nameof(ShortBot), MethodBase.GetCurrentMethod()?.Name, ex);
            }
        }

        public async Task CloseBuy(string symbol, decimal price, decimal quantity)
        {
            try
            {
                var limitPrice = StockUtil.GetPriceByRoe(MercuryTradingModel.Enums.PositionSide.Long, price, 1m).ToValidPrice(symbol); // +0 ~ +1%

                var result = await BinanceClients.CloseBuy(symbol, limitPrice, quantity).ConfigureAwait(false);
                if (result.Success)
                {
                    Common.AddHistory($"Close Buy {symbol}, {price}, {quantity}");
                }
                else
                {
                    Common.AddHistory($"Close Buy Error: {result.Error?.Message}");
                }
            }
            catch (Exception ex)
            {
                Logger.Log(nameof(ShortBot), MethodBase.GetCurrentMethod()?.Name, ex);
            }
        }

        public async Task SetTakeProfit(string symbol, decimal price, decimal quantity)
        {
            try
            {
                var takePrice = price.ToValidPrice(symbol);
                var limitPrice = StockUtil.GetPriceByRoe(MercuryTradingModel.Enums.PositionSide.Long, price, 1m).ToValidPrice(symbol); // +0 ~ +1%

                var result = await BinanceClients.SetShortTakeProfit(symbol, limitPrice, quantity, takePrice).ConfigureAwait(false);
                if (result.Success)
                {
                    Common.AddHistory($"Set Take Profit {symbol}, {price}, {quantity}");
                }
                else
                {
                    Common.AddHistory($"Set Take Profit Error: {result.Error?.Message}");
                }
            }
            catch (Exception ex)
            {
                Logger.Log(nameof(ShortBot), MethodBase.GetCurrentMethod()?.Name, ex);
            }
        }

        public async Task SetStopLoss(string symbol, decimal price, decimal quantity)
        {
            try
            {
                var stopPrice = price.ToValidPrice(symbol);
                var limitPrice = StockUtil.GetPriceByRoe(MercuryTradingModel.Enums.PositionSide.Long, price, 1m).ToValidPrice(symbol); // +0 ~ +1%

                var result = await BinanceClients.SetShortStopLoss(symbol, limitPrice, quantity, stopPrice).ConfigureAwait(false);
                if (result.Success)
                {
                    Common.AddHistory($"Set Stop Loss {symbol}, {price}, {quantity}");
                }
                else
                {
                    Common.AddHistory($"Set Stop Loss Error: {result.Error?.Message}");
                }
            }
            catch (Exception ex)
            {
                Logger.Log(nameof(ShortBot), MethodBase.GetCurrentMethod()?.Name, ex);
            }
        }

        public void MockOpenBuy(string symbol, decimal price, decimal quantity)
        {
            try
            {
                Common.MockPositions.Add(new BinancePosition(symbol, "Long", 0, price, price, quantity, Leverage));
                Common.AddHistory($"Open Buy {symbol}, {price}, {quantity}");
            }
            catch (Exception ex)
            {
                Logger.Log(nameof(ShortBot), MethodBase.GetCurrentMethod()?.Name, ex);
            }
        }

        public void MockCloseSell(string symbol, decimal price, decimal quantity)
        {
            try
            {
                var position = Common.MockPositions.Find(a => a.Symbol.Equals(symbol));
                Common.MockPositions.Remove(position);
                Common.AddHistory($"Close Sell {symbol}, {price}, {quantity}");
            }
            catch (Exception ex)
            {
                Logger.Log(nameof(ShortBot), MethodBase.GetCurrentMethod()?.Name, ex);
            }
        }
    }
}
