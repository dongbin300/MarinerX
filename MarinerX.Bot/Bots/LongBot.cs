using Binance.Net.Enums;
using CryptoModel.Maths;
using MarinerX.Bot.Clients;
using MarinerX.Bot.Extensions;
using MarinerX.Bot.Models;

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace MarinerX.Bot.Bots
{
    public class LongBot : Bot
    {
        #region Entry
        public bool IsRunning { get; set; }
        public decimal BaseOrderSize { get; set; }
        public decimal TargetRoe { get; set; }
        public int Leverage { get; set; }
        public int MaxActiveDeals { get; set; }

        private PositionSide side => PositionSide.Long;
        private List<string> DealingSymbols = new();

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
        #endregion

        private bool IsEntryTs2LongBit(List<ChartInfo> charts)
        {
            int condition = 0;
            for (int i = charts.Count - 2; i >= 0; i--) // 이전 봉 기준
            {
                var chart = charts[i];

                switch (condition)
                {
                    case 0:
                        if (chart.Supertrend1 > 0 && chart.Supertrend2 > 0 && chart.Supertrend3 > 0)
                        {
                            condition = 1;
                        }
                        else
                        {
                            return false;
                        }
                        break;
                    case 1:
                        if (chart.Supertrend1 < 0 && chart.Supertrend2 > 0 && chart.Supertrend3 > 0)
                        {
                            condition = 2;
                        }
                        else
                        {
                            return false;
                        }
                        break;
                    case 2:
                        if (chart.Supertrend1 < 0 && chart.Supertrend2 > 0 && chart.Supertrend3 > 0)
                        {

                        }
                        else if (chart.Supertrend1 > 0 && chart.Supertrend2 > 0 && chart.Supertrend3 > 0)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                        break;
                }
            }
            return false;
        }

        public async Task Evaluate()
        {
            try
            {
                if (Common.LongPositions.Count >= MaxActiveDeals) // 동시 거래 수 MAX
                {
                    return;
                }

                foreach (var pairQuote in Common.PairQuotes)
                {
                    var symbol = pairQuote.Symbol;
                    var c0 = pairQuote.Charts[^1]; // 현재 정보
                    var c1 = pairQuote.Charts[^2]; // 1봉전 정보

                    if (!Common.IsLongPositioning(symbol)) // 포지션이 없으면
                    {
                        if (DateTime.Now.Minute == 0 || DateTime.Now.Minute == 30) // 캔들이 갱신되는 순간에만 진입
                        {
                            if (Common.IsCoolTime(symbol, side)) // 정리한지 시간이 별로 안 지났으면 스킵
                            {
                                continue;
                            }

                            if (DealingSymbols.Contains(symbol)) // 이미 주문하고 있는 중이면
                            {
                                continue;
                            }

                            DealingSymbols.Add(symbol);
                            try
                            {
                                // 진입 조건에 부합하면
                                if (IsEntryTs2LongBit(pairQuote.Charts))
                                {
                                    var price = c0.Quote.Close;
                                    var quantity = (BaseOrderSize / price).ToValidQuantity(symbol);
                                    var halfQuantity = (quantity / 2).ToValidQuantity(symbol);
                                    Common.AddHistory("Long Bot", $"ST1 {c1.Supertrend1}, ST2 {c1.Supertrend2}, ST3 {c1.Supertrend3}");
                                    if (await OpenBuy(symbol, price, quantity).ConfigureAwait(false))
                                    {
                                        var stopLossPrice = (decimal)Math.Abs(c1.Supertrend2);
                                        await SetStopLoss(symbol, stopLossPrice, quantity).ConfigureAwait(false);
                                        var takeProfitPrice = 2 * price - stopLossPrice;
                                        await SetTakeProfit(symbol, takeProfitPrice, halfQuantity).ConfigureAwait(false);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Logger.Log(nameof(LongBot), MethodBase.GetCurrentMethod()?.Name, ex);
                            }
                            DealingSymbols.Remove(symbol);
                        }
                    }
                    else // 포지션이 있으면
                    {
                        var stopLossOrder = Common.GetOrder(symbol, side, FuturesOrderType.Stop);
                        if (stopLossOrder == null)
                        {
                            continue;
                        }

                        if ((DateTime.UtcNow - stopLossOrder.CreateTime).TotalSeconds < 30)
                        {
                            continue;
                        }

                        var takeProfitOrder = Common.GetOrder(symbol, side, FuturesOrderType.TakeProfit);
                        if (takeProfitOrder == null && c1.Supertrend1 < 0)
                        {
                            var position = Common.GetPosition(symbol, side);
                            if (position == null)
                            {
                                continue;
                            }
                            var price = c0.Quote.Close;
                            var quantity = Math.Abs(position.Quantity);
                            await CloseSell(symbol, price, quantity).ConfigureAwait(false);
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
                        var result = await BinanceClients.Api.UsdFuturesApi.Trading.CancelOrderAsync(order.Symbol, order.Id).ConfigureAwait(false);
                        if (result.Success)
                        {
                            Common.AddHistory("Long Bot", $"Cancel Order {order.Symbol}, {order.Id}");
                        }
                        else
                        {
                            Common.AddHistory("Long Bot", $"Cancel Order {order.Symbol}, Error: {result.Error?.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(nameof(LongBot), MethodBase.GetCurrentMethod()?.Name, ex);
            }
        }

        public async Task<bool> OpenBuy(string symbol, decimal price, decimal quantity)
        {
            try
            {
                await BinanceClients.Api.UsdFuturesApi.Account.ChangeInitialLeverageAsync(symbol, Leverage); // 레버리지 설정
                var limitPrice = Calculator.TargetPrice(side, price, 0.25m).ToValidPrice(symbol); // +0 ~ +0.25%

                var result = await BinanceClients.OpenBuy(symbol, limitPrice, quantity).ConfigureAwait(false);
                if (result.Success)
                {
                    Common.AddHistory("Long Bot", $"Open Buy {symbol}, {price}, {quantity}");
                    return true;
                }
                else
                {
                    Common.AddHistory("Long Bot", $"Open Buy {symbol}, Error: {result.Error?.Message}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Logger.Log(nameof(LongBot), MethodBase.GetCurrentMethod()?.Name, ex);
                return false;
            }
        }

        public async Task CloseSell(string symbol, decimal price, decimal quantity)
        {
            try
            {
                var limitPrice = Calculator.TargetPrice(side, price, -1m).ToValidPrice(symbol); // -0 ~ -1%

                var result = await BinanceClients.CloseSell(symbol, limitPrice, quantity).ConfigureAwait(false);
                if (result.Success)
                {
                    Common.AddHistory("Long Bot", $"Close Sell {symbol}, {price}, {quantity}");
                }
                else
                {
                    Common.AddHistory("Long Bot", $"Close Sell {symbol}, Error: {result.Error?.Message}");
                }
            }
            catch (Exception ex)
            {
                Logger.Log(nameof(LongBot), MethodBase.GetCurrentMethod()?.Name, ex);
            }
        }

        public async Task SetTakeProfit(string symbol, decimal price, decimal quantity)
        {
            try
            {
                var takePrice = price.ToValidPrice(symbol);
                var limitPrice = Calculator.TargetPrice(side, price, -1m).ToValidPrice(symbol); // -0 ~ -1%

                var result = await BinanceClients.SetLongTakeProfit(symbol, limitPrice, quantity, takePrice).ConfigureAwait(false);
                if (result.Success)
                {
                    Common.AddHistory("Long Bot", $"Set Take Profit {symbol}, {price}, {quantity}");
                }
                else
                {
                    Common.AddHistory("Long Bot", $"Set Take Profit {symbol}, Error: {result.Error?.Message}");
                }
            }
            catch (Exception ex)
            {
                Logger.Log(nameof(LongBot), MethodBase.GetCurrentMethod()?.Name, ex);
            }
        }

        public async Task SetStopLoss(string symbol, decimal price, decimal quantity)
        {
            try
            {
                var stopPrice = price.ToValidPrice(symbol);
                var limitPrice = Calculator.TargetPrice(side, price, -1m).ToValidPrice(symbol); // -0 ~ -1%

                var result = await BinanceClients.SetLongStopLoss(symbol, limitPrice, quantity, stopPrice).ConfigureAwait(false);
                if (result.Success)
                {
                    Common.AddHistory("Long Bot", $"Set Stop Loss {symbol}, {price}, {quantity}");
                }
                else
                {
                    Common.AddHistory("Long Bot", $"Set Stop Loss {symbol}, Error: {result.Error?.Message}");
                }
            }
            catch (Exception ex)
            {
                Logger.Log(nameof(LongBot), MethodBase.GetCurrentMethod()?.Name, ex);
            }
        }

        /*
        #region Mock
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
                Logger.Log(nameof(LongBot), MethodBase.GetCurrentMethod()?.Name, ex);
            }
        }

        public void MockOpenBuy(string symbol, decimal price, decimal quantity)
        {
            try
            {
                Common.MockPositions.Add(new BinancePosition(symbol, "Long", 0, price, price, quantity, Leverage));
                Common.AddHistory("Long Bot(Mock)", $"Open Buy {symbol}, {price}, {quantity}");
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
                var position = Common.MockPositions.Find(a => a.Symbol.Equals(symbol));
                Common.MockPositions.Remove(position);
                Common.AddHistory("Long Bot(Mock)", $"Close Sell {symbol}, {price}, {quantity}");
            }
            catch (Exception ex)
            {
                Logger.Log(nameof(LongBot), MethodBase.GetCurrentMethod()?.Name, ex);
            }
        }
        #endregion
        */
    }
}
