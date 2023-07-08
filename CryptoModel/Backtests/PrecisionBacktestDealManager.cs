using Binance.Net.Enums;

using CryptoModel.Charts;
using CryptoModel.Maths;

namespace CryptoModel.Backtests
{
    public class PrecisionBacktestDealManager
    {
        public int MaxActiveDeals { get; set; }
        public decimal TakeProfitRoe { get; set; }
        public decimal StopLossRoe { get; set; }
        public decimal Fee { get; set; }

        public int Win { get; set; } = 0;
        public int Lose { get; set; } = 0;
        public decimal WinRate => Win + Lose == 0 ? 0 : (decimal)Win / (Win + Lose) * 100;
        public decimal Profit => TakeProfitRoe - Fee;
        public decimal Loss => StopLossRoe - Fee;
        public decimal SimplePnl => Profit * Win + Loss * Lose;

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int BacktestDays => (int)(EndDate - StartDate).TotalDays;

        public List<string> MonitoringSymbols { get; set; } = new();
        public Dictionary<string, List<ChartInfo>> Charts { get; set; } = new();
        public List<Position> Positions { get; set; } = new();
        public List<PositionHistory> PositionHistories { get; set; } = new();
        public int LongPositionCount => Positions.Count(x => x.Side.Equals(PositionSide.Long));
        public int ShortPositionCount => Positions.Count(x => x.Side.Equals(PositionSide.Short));

        public decimal Money = 10000;
        public decimal BaseOrderSize = 100;
        public decimal FeeSize = 0.02m;
        public decimal EstimatedMoney => Money
            + Positions.Where(x => x.Side.Equals(PositionSide.Long)).Sum(x => x.EntryPrice * x.Quantity)
            - Positions.Where(x => x.Side.Equals(PositionSide.Short)).Sum(x => x.EntryPrice * x.Quantity);

        public PrecisionBacktestDealManager(DateTime startDate, DateTime endDate, int maxActiveDeals, decimal takeProfitRoe, decimal stopLossRoe, decimal fee)
        {
            StartDate = startDate;
            EndDate = endDate;
            MaxActiveDeals = maxActiveDeals;
            TakeProfitRoe = takeProfitRoe;
            StopLossRoe = stopLossRoe;
            Fee = fee;
        }

        public void ConcatenateChart(Dictionary<string, ChartInfo> charts)
        {
            foreach (var chart in charts)
            {
                if (Charts.ContainsKey(chart.Key))
                {
                    Charts[chart.Key].Add(chart.Value);
                }
                else
                {
                    Charts.Add(chart.Key, new List<ChartInfo> { chart.Value });
                }
            }
        }

        public void RemoveOldChart()
        {
            foreach (var chart in Charts)
            {
                chart.Value.RemoveRange(0, 12);
            }
        }

        #region LSMA
        public void CalculateIndicatorsLsma()
        {
            foreach (var chart in Charts)
            {
                var quotes = chart.Value.Select(x => x.Quote);
                var r1 = quotes.GetLsma(10).Select(x => x.Lsma);
                var r2 = quotes.GetLsma(30).Select(x => x.Lsma);
                var r3 = quotes.GetRsi(14).Select(x => x.Rsi);
                for (int i = 0; i < chart.Value.Count; i++)
                {
                    var _chart = chart.Value[i];
                    _chart.Lsma1 = r1.ElementAt(i);
                    _chart.Lsma2 = r2.ElementAt(i);
                    _chart.Rsi = r3.ElementAt(i);
                }
            }
        }

        public List<ChartInfo> FastCalculateIndicatorsLsma(string symbol, Quote lastQuote)
        {
            var chart = Charts[symbol];
            var quotes = chart.Select(x => x.Quote);
            var r1 = quotes.TakeLast(12).SkipLast(1).Concat(new[] { lastQuote }).GetLsma(10).Select(x => x.Lsma);
            var r2 = quotes.TakeLast(32).SkipLast(1).Concat(new[] { lastQuote }).GetLsma(30).Select(x => x.Lsma);

            int p = 0;
            var result = new List<ChartInfo>();
            for (int i = 0; i < 3; i++)
            {
                var info = new ChartInfo("", new Quote())
                {
                    Lsma1 = r1.ElementAt(9 + p),
                    Lsma2 = r2.ElementAt(29 + p),
                };
                p++;
                result.Add(info);
            }

            return result;
        }

        public void EvaluateLsmaLongInstant(double rsiThreshold = 40)
        {
            var side = PositionSide.Long;
            foreach (var symbol in MonitoringSymbols)
            {
                var position = Positions.Find(x => x.Symbol.Equals(symbol) && x.Side.Equals(side));

                var charts = Charts[symbol];
                var c0 = charts[^1];
                var c1 = charts[^2];
                var c2 = charts[^3];
                var c3 = charts[^4];
                var c4 = charts[^5];

                // 포지션이 없으면
                if (position == null)
                {
                    if (LongPositionCount >= MaxActiveDeals)
                    {
                        continue;
                    }
                    // RSI 40라인을 골든 크로스 이후, 3봉 이내에 LSMA 10이 30을 골든 크로스하면 진입
                    if ((c0.Rsi > rsiThreshold && c1.Rsi < rsiThreshold) || (c1.Rsi > rsiThreshold && c2.Rsi < rsiThreshold) || (c2.Rsi > rsiThreshold && c3.Rsi < rsiThreshold))
                    {
                        var per = 0m;
                        var testPrice = c0.Quote.Open;
                        while (testPrice <= c0.Quote.High)
                        {
                            var quote = new Quote
                            {
                                Date = c0.DateTime,
                                Open = c0.Quote.Open,
                                High = c0.Quote.High,
                                Low = c0.Quote.Low,
                                Close = testPrice,
                                Volume = c0.Quote.Volume
                            };
                            var newCharts = FastCalculateIndicatorsLsma(symbol, quote);
                            var nc0 = newCharts[^1];
                            var nc1 = newCharts[^2];
                            if (nc0.Lsma1 > nc0.Lsma2 && nc1.Lsma1 < nc1.Lsma2)
                            {
                                var price = testPrice;
                                Positions.Add(new Position(c0.DateTime, symbol, side, price));
                            }

                            per += 0.05m;
                            testPrice = Calculator.TargetPrice(side, c0.Quote.Open, per);
                        }
                    }
                }
                // 포지션이 있으면
                else
                {
                    decimal minRoe = 0;
                    decimal maxRoe = 0;

                    var low = Calculator.Roe(side, position.EntryPrice, c0.Quote.Low);
                    var high = Calculator.Roe(side, position.EntryPrice, c0.Quote.High);

                    if (low < high)
                    {
                        minRoe = low;
                        maxRoe = high;
                    }
                    else
                    {
                        minRoe = high;
                        maxRoe = low;
                    }

                    // 손절
                    if (minRoe <= StopLossRoe)
                    {
                        Positions.Remove(position);
                        PositionHistories.Add(new PositionHistory(c0.DateTime, position.Time, symbol, side, PositionResult.Lose));
                        Lose++;
                    }
                    // 익절
                    else if (maxRoe >= TakeProfitRoe)
                    {
                        Positions.Remove(position);
                        PositionHistories.Add(new PositionHistory(c0.DateTime, position.Time, symbol, side, PositionResult.Win));
                        Win++;
                    }
                }
            }
        }

        public void EvaluateLsmaShortInstant(double rsiThreshold = 60)
        {
            var side = PositionSide.Short;
            foreach (var symbol in MonitoringSymbols)
            {
                var position = Positions.Find(x => x.Symbol.Equals(symbol) && x.Side.Equals(side));

                var charts = Charts[symbol];
                var c0 = charts[^1];
                var c1 = charts[^2];
                var c2 = charts[^3];
                var c3 = charts[^4];
                var c4 = charts[^5];

                // 포지션이 없으면
                if (position == null)
                {
                    if (ShortPositionCount >= MaxActiveDeals)
                    {
                        continue;
                    }
                    // RSI 40라인을 골든 크로스 이후, 3봉 이내에 LSMA 10이 30을 골든 크로스하면 진입
                    if ((c0.Rsi < rsiThreshold && c1.Rsi > rsiThreshold) || (c1.Rsi < rsiThreshold && c2.Rsi > rsiThreshold) || (c2.Rsi < rsiThreshold && c3.Rsi > rsiThreshold))
                    {
                        var per = 0m;
                        var testPrice = c0.Quote.Open;
                        while (testPrice >= c0.Quote.Low)
                        {
                            var quote = new Quote
                            {
                                Date = c0.DateTime,
                                Open = c0.Quote.Open,
                                High = c0.Quote.High,
                                Low = c0.Quote.Low,
                                Close = testPrice,
                                Volume = c0.Quote.Volume
                            };
                            var newCharts = FastCalculateIndicatorsLsma(symbol, quote);
                            var nc0 = newCharts[^1];
                            var nc1 = newCharts[^2];
                            if (nc0.Lsma1 < nc0.Lsma2 && nc1.Lsma1 > nc1.Lsma2)
                            {
                                var price = testPrice;
                                Positions.Add(new Position(c0.DateTime, symbol, side, price));
                            }

                            per += 0.05m;
                            testPrice = Calculator.TargetPrice(side, c0.Quote.Open, per);
                        }
                    }
                }
                // 포지션이 있으면
                else
                {
                    decimal minRoe = 0;
                    decimal maxRoe = 0;

                    var low = Calculator.Roe(side, position.EntryPrice, c0.Quote.Low);
                    var high = Calculator.Roe(side, position.EntryPrice, c0.Quote.High);

                    if (low < high)
                    {
                        minRoe = low;
                        maxRoe = high;
                    }
                    else
                    {
                        minRoe = high;
                        maxRoe = low;
                    }

                    // 손절
                    if (minRoe <= StopLossRoe)
                    {
                        Positions.Remove(position);
                        PositionHistories.Add(new PositionHistory(c0.DateTime, position.Time, symbol, side, PositionResult.Lose));
                        Lose++;
                    }
                    // 익절
                    else if (maxRoe >= TakeProfitRoe)
                    {
                        Positions.Remove(position);
                        PositionHistories.Add(new PositionHistory(c0.DateTime, position.Time, symbol, side, PositionResult.Win));
                        Win++;
                    }
                }
            }
        }

        public void EvaluateLsmaLongNextCandle(double rsiThreshold = 40)
        {
            var side = PositionSide.Long;
            foreach (var symbol in MonitoringSymbols)
            {
                var position = Positions.Find(x => x.Symbol.Equals(symbol) && x.Side.Equals(side));

                var charts = Charts[symbol];
                var c0 = charts[^1];
                var c1 = charts[^2];
                var c2 = charts[^3];
                var c3 = charts[^4];
                var c4 = charts[^5];

                // 포지션이 없으면
                if (position == null)
                {
                    if (LongPositionCount >= MaxActiveDeals)
                    {
                        continue;
                    }
                    // RSI 40라인을 골든 크로스 이후, 3봉 이내에 LSMA 10이 30을 골든 크로스하면 진입
                    if ((c1.Rsi > rsiThreshold && c2.Rsi < rsiThreshold) || (c2.Rsi > rsiThreshold && c3.Rsi < rsiThreshold) || (c3.Rsi > rsiThreshold && c4.Rsi < rsiThreshold))
                    {
                        if (c1.Lsma1 > c1.Lsma2 && c2.Lsma1 < c2.Lsma2)
                        {
                            var price = c0.Quote.Open;
                            Positions.Add(new Position(c0.DateTime, symbol, side, price));
                        }
                    }
                }
                // 포지션이 있으면
                else
                {
                    decimal minRoe = 0;
                    decimal maxRoe = 0;

                    var low = Calculator.Roe(side, position.EntryPrice, c0.Quote.Low);
                    var high = Calculator.Roe(side, position.EntryPrice, c0.Quote.High);

                    if (low < high)
                    {
                        minRoe = low;
                        maxRoe = high;
                    }
                    else
                    {
                        minRoe = high;
                        maxRoe = low;
                    }

                    // 손절
                    if (minRoe <= StopLossRoe)
                    {
                        Positions.Remove(position);
                        PositionHistories.Add(new PositionHistory(c0.DateTime, position.Time, symbol, side, PositionResult.Lose));
                        Lose++;
                    }
                    // 익절
                    else if (maxRoe >= TakeProfitRoe)
                    {
                        Positions.Remove(position);
                        PositionHistories.Add(new PositionHistory(c0.DateTime, position.Time, symbol, side, PositionResult.Win));
                        Win++;
                    }
                }
            }
        }

        public void EvaluateLsmaShortNextCandle(double rsiThreshold = 60)
        {
            var side = PositionSide.Short;
            foreach (var symbol in MonitoringSymbols)
            {
                var position = Positions.Find(x => x.Symbol.Equals(symbol) && x.Side.Equals(side));

                var charts = Charts[symbol];
                var c0 = charts[^1];
                var c1 = charts[^2];
                var c2 = charts[^3];
                var c3 = charts[^4];
                var c4 = charts[^5];

                // 포지션이 없으면
                if (position == null)
                {
                    if (ShortPositionCount >= MaxActiveDeals)
                    {
                        continue;
                    }
                    // RSI 40라인을 골든 크로스 이후, 3봉 이내에 LSMA 10이 30을 골든 크로스하면 진입
                    if ((c1.Rsi < rsiThreshold && c2.Rsi > rsiThreshold) || (c2.Rsi < rsiThreshold && c3.Rsi > rsiThreshold) || (c3.Rsi < rsiThreshold && c4.Rsi > rsiThreshold))
                    {
                        if (c1.Lsma1 < c1.Lsma2 && c2.Lsma1 > c2.Lsma2)
                        {
                            var price = c0.Quote.Open;
                            Positions.Add(new Position(c0.DateTime, symbol, side, price));
                        }
                    }
                }
                // 포지션이 있으면
                else
                {
                    decimal minRoe = 0;
                    decimal maxRoe = 0;

                    var low = Calculator.Roe(side, position.EntryPrice, c0.Quote.Low);
                    var high = Calculator.Roe(side, position.EntryPrice, c0.Quote.High);

                    if (low < high)
                    {
                        minRoe = low;
                        maxRoe = high;
                    }
                    else
                    {
                        minRoe = high;
                        maxRoe = low;
                    }

                    // 손절
                    if (minRoe <= StopLossRoe)
                    {
                        Positions.Remove(position);
                        PositionHistories.Add(new PositionHistory(c0.DateTime, position.Time, symbol, side, PositionResult.Lose));
                        Lose++;
                    }
                    // 익절
                    else if (maxRoe >= TakeProfitRoe)
                    {
                        Positions.Remove(position);
                        PositionHistories.Add(new PositionHistory(c0.DateTime, position.Time, symbol, side, PositionResult.Win));
                        Win++;
                    }
                }
            }
        }
        #endregion

        #region Triple Supertrend (TS)
        /*
         * Triple Supertrend 매매법
         * 롱 진입
         * - 1. Stochastic RSI K선이 20 위로 돌파
         * - 2. EMA 200 선 위에 존재
         * - 3. TS 연두색 선이 2개 이상 존재
         * 롱 손절
         * - TS 연두색 선중 가장 낮은 것
         * 롱 익절
         * - 손절:익절비 = 1:2
         * 
         * 숏 진입
         * - 1. Stochastic RSI K선이 80 아래로 돌파
         * - 2. EMA 200 선 아래에 존재
         * - 3. TS 빨간색 선이 2개 이상 존재
         * 숏 손절
         * - TS 빨간색 선중 가장 높은 것
         * 숏 익절
         * - 손절:익절비 = 1:2
         */
        public void CalculateIndicatorsTs()
        {
            foreach (var chart in Charts)
            {
                var quotes = chart.Value.Select(x => x.Quote);
                var r1 = quotes.GetEma(200).Select(x => x.Ema);
                var r2 = quotes.GetStochasticRsi(3, 3, 14, 14).Select(x => x.K);
                var ts = quotes.GetTripleSupertrend(10, 1, 11, 2, 12, 3);
                var r3 = ts.Select(x => x.Supertrend1);
                var r4 = ts.Select(x => x.Supertrend2);
                var r5 = ts.Select(x => x.Supertrend3);
                for (int i = 0; i < chart.Value.Count; i++)
                {
                    var _chart = chart.Value[i];
                    _chart.Ema1 = r1.ElementAt(i);
                    _chart.K = r2.ElementAt(i);
                    _chart.Supertrend1 = r3.ElementAt(i);
                    _chart.Supertrend2 = r4.ElementAt(i);
                    _chart.Supertrend3 = r5.ElementAt(i);
                }
            }
        }

        public void CalculateIndicatorsTs2()
        {
            foreach (var chart in Charts)
            {
                var quotes = chart.Value.Select(x => x.Quote);
                var ts = quotes.GetTripleSupertrend(10, 1.2, 10, 3, 10, 10);
                var r1 = ts.Select(x => x.Supertrend1);
                var r2 = ts.Select(x => x.Supertrend2);
                var r3 = ts.Select(x => x.Supertrend3);
                for (int i = 0; i < chart.Value.Count; i++)
                {
                    var _chart = chart.Value[i];
                    _chart.Supertrend1 = r1.ElementAt(i);
                    _chart.Supertrend2 = r2.ElementAt(i);
                    _chart.Supertrend3 = r3.ElementAt(i);
                }
            }
        }

        public List<ChartInfo> FastCalculateIndicatorsTs(string symbol, Quote lastQuote)
        {
            var chart = Charts[symbol];
            var quotes = chart.Select(x => x.Quote);
            var r1 = quotes.TakeLast(202).SkipLast(1).Concat(new[] { lastQuote }).GetEma(200).Select(x => x.Ema);
            var r2 = quotes.TakeLast(20).SkipLast(1).Concat(new[] { lastQuote }).GetStochasticRsi(3, 3, 14, 14).Select(x => x.K);
            var ts = quotes.TakeLast(20).SkipLast(1).Concat(new[] { lastQuote }).GetTripleSupertrend(10, 1, 11, 2, 12, 3);
            var r3 = ts.Select(x => x.Supertrend1);
            var r4 = ts.Select(x => x.Supertrend2);
            var r5 = ts.Select(x => x.Supertrend3);

            int p = 0;
            var result = new List<ChartInfo>();
            for (int i = 0; i < 2; i++)
            {
                var info = new ChartInfo("", new Quote())
                {
                    Ema1 = r1.ElementAt(200 + p),
                    K = r2.ElementAt(18 + p),
                    Supertrend1 = r3.ElementAt(18 + p),
                    Supertrend2 = r4.ElementAt(18 + p),
                    Supertrend3 = r5.ElementAt(18 + p),
                };
                p++;
                result.Add(info);
            }

            return result;
        }

        public List<ChartInfo> FastCalculateIndicatorsTs2(string symbol, Quote lastQuote)
        {
            var chart = Charts[symbol];
            var quotes = chart.Select(x => x.Quote);
            var ts = quotes.TakeLast(20).SkipLast(1).Concat(new[] { lastQuote }).GetTripleSupertrend(10, 1.2, 10, 3, 10, 10);
            var r1 = ts.Select(x => x.Supertrend1);
            var r2 = ts.Select(x => x.Supertrend2);
            var r3 = ts.Select(x => x.Supertrend3);

            int p = 0;
            var result = new List<ChartInfo>();
            for (int i = 0; i < 2; i++)
            {
                var info = new ChartInfo("", new Quote())
                {
                    Supertrend1 = r1.ElementAt(18 + p),
                    Supertrend2 = r2.ElementAt(18 + p),
                    Supertrend3 = r3.ElementAt(18 + p),
                };
                p++;
                result.Add(info);
            }

            return result;
        }

        private bool IsTwoGreenSignal(ChartInfo info)
        {
            var count = 0;
            count += info.Supertrend1 > 0 ? 1 : 0;
            count += info.Supertrend2 > 0 ? 1 : 0;
            count += info.Supertrend3 > 0 ? 1 : 0;
            return count >= 2;
        }

        private bool IsTwoRedSignal(ChartInfo info)
        {
            var count = 0;
            count += info.Supertrend1 < 0 ? 1 : 0;
            count += info.Supertrend2 < 0 ? 1 : 0;
            count += info.Supertrend3 < 0 ? 1 : 0;
            return count >= 2;
        }

        private double MinGreenSignal(ChartInfo info)
        {
            var supertrends = new[] { info.Supertrend1, info.Supertrend2, info.Supertrend3 };
            return supertrends.Where(x => x > 0).Min();
        }

        private double MaxRedSignal(ChartInfo info)
        {
            var supertrends = new[] { info.Supertrend1, info.Supertrend2, info.Supertrend3 };
            return supertrends.Where(x => x < 0).Select(x => Math.Abs(x)).Max();
        }

        /// <summary>
        /// 15분봉 기준 EMA선과 5% 이내일 경우 참
        /// </summary>
        /// <param name="side"></param>
        /// <param name="ema"></param>
        /// <param name="price"></param>
        /// <returns></returns>
        private bool IsNear(PositionSide side, decimal ema, decimal price)
        {
            return Calculator.Roe(side, ema, price) <= 5.0m;
        }

        public void EvaluateTsLongInstant()
        {
            var side = PositionSide.Long;
            foreach (var symbol in MonitoringSymbols)
            {
                var position = Positions.Find(x => x.Symbol.Equals(symbol) && x.Side.Equals(side));

                var charts = Charts[symbol];
                var c0 = charts[^1];

                // 포지션이 없으면
                if (position == null)
                {
                    if (LongPositionCount >= MaxActiveDeals)
                    {
                        continue;
                    }
                    var per = 0m;
                    var testPrice = c0.Quote.Open;
                    while (testPrice <= c0.Quote.High)
                    {
                        var quote = new Quote
                        {
                            Date = c0.DateTime,
                            Open = c0.Quote.Open,
                            High = c0.Quote.High,
                            Low = c0.Quote.Low,
                            Close = testPrice,
                            Volume = c0.Quote.Volume
                        };
                        var newCharts = FastCalculateIndicatorsTs(symbol, quote);
                        var nc0 = newCharts[^1];
                        var nc1 = newCharts[^2];
                        // TS 연두색 선이 2개 이상 존재하고 EMA 200 선 위에 있고 StochRSI K값이 20 골든크로스 하면
                        if (IsTwoGreenSignal(nc0) && (double)quote.Close > nc0.Ema1 && nc0.K > 20 && nc1.K < 20)
                        {
                            var price = testPrice;
                            var stopLossPrice = (decimal)MinGreenSignal(nc0);
                            var takeProfitPrice = price + price - stopLossPrice;
                            var quantity = BaseOrderSize / price;
                            Money -= price * quantity;
                            var newPosition = new Position(c0.DateTime, symbol, side, price)
                            {
                                StopLossPrice = stopLossPrice,
                                TakeProfitPrice = takeProfitPrice,
                                Quantity = quantity
                            };
                            Positions.Add(newPosition);

                            break;
                        }

                        per += 0.05m;
                        testPrice = Calculator.TargetPrice(side, c0.Quote.Open, per);
                    }
                }
                // 포지션이 있으면
                else
                {
                    // 손절
                    if (c0.Quote.Close <= position.StopLossPrice)
                    {
                        if (position.Stage == 0) // 익절없이 손절
                        {
                            Money += position.StopLossPrice * position.Quantity;
                            Positions.Remove(position);
                            PositionHistories.Add(new PositionHistory(c0.DateTime, position.Time, symbol, side, PositionResult.Lose));
                            Lose++;
                            Money -= FeeSize;
                        }
                        else // 익절하고 손절
                        {
                            Money += position.StopLossPrice * position.Quantity;
                            Positions.Remove(position);
                            PositionHistories.Add(new PositionHistory(c0.DateTime, position.Time, symbol, side, PositionResult.LittleWin));
                            Win++;
                            Money -= FeeSize;
                        }
                    }
                    // 1차 익절
                    else if (position.Stage == 0 && c0.Quote.Close >= position.TakeProfitPrice)
                    {
                        var quantity = position.Quantity / 2;
                        Money += position.TakeProfitPrice * quantity;
                        position.Quantity -= quantity;
                        position.Stage = 1;

                        // TP/SL 재설정
                        var stopLossPrice = position.EntryPrice;
                        var takeProfitPrice = c0.Quote.Close + c0.Quote.Close - stopLossPrice;
                        position.StopLossPrice = stopLossPrice;
                        position.TakeProfitPrice = takeProfitPrice;
                    }
                    // 2차 익절
                    else if (position.Stage == 1 && c0.Quote.Close >= position.TakeProfitPrice)
                    {
                        Money += position.TakeProfitPrice * position.Quantity;

                        Positions.Remove(position);
                        PositionHistories.Add(new PositionHistory(c0.DateTime, position.Time, symbol, side, PositionResult.Win));
                        Win++;
                        Money -= FeeSize;
                    }
                }
            }
        }

        public void EvaluateTsShortInstant()
        {
            var side = PositionSide.Short;
            foreach (var symbol in MonitoringSymbols)
            {
                var position = Positions.Find(x => x.Symbol.Equals(symbol) && x.Side.Equals(side));

                var charts = Charts[symbol];
                var c0 = charts[^1];

                // 포지션이 없으면
                if (position == null)
                {
                    if (ShortPositionCount >= MaxActiveDeals)
                    {
                        continue;
                    }
                    var per = 0m;
                    var testPrice = c0.Quote.Open;
                    while (testPrice >= c0.Quote.Low)
                    {
                        var quote = new Quote
                        {
                            Date = c0.DateTime,
                            Open = c0.Quote.Open,
                            High = c0.Quote.High,
                            Low = c0.Quote.Low,
                            Close = testPrice,
                            Volume = c0.Quote.Volume
                        };
                        var newCharts = FastCalculateIndicatorsTs(symbol, quote);
                        var nc0 = newCharts[^1];
                        var nc1 = newCharts[^2];
                        // TS 빨간색 선이 2개 이상 존재하고 EMA 200 선 아래에 있고 StochRSI K값이 80 데드크로스 하면
                        if (IsTwoRedSignal(nc0) && (double)quote.Close < nc0.Ema1 && nc0.K < 80 && nc1.K > 80)
                        {
                            var price = testPrice;
                            var stopLossPrice = (decimal)MaxRedSignal(nc0);
                            var takeProfitPrice = price + price - stopLossPrice;
                            var quantity = BaseOrderSize / price;
                            Money -= price * quantity;
                            var newPosition = new Position(c0.DateTime, symbol, side, price)
                            {
                                StopLossPrice = stopLossPrice,
                                TakeProfitPrice = takeProfitPrice,
                                Quantity = quantity
                            };
                            Positions.Add(newPosition);

                            break;
                        }

                        per += 0.05m;
                        testPrice = Calculator.TargetPrice(side, c0.Quote.Open, per);
                    }
                }
                // 포지션이 있으면
                else
                {
                    // 손절
                    if (c0.Quote.Close >= position.StopLossPrice)
                    {
                        if (position.Stage == 0) // 익절없이 손절
                        {
                            Money += position.StopLossPrice * position.Quantity;
                            Positions.Remove(position);
                            PositionHistories.Add(new PositionHistory(c0.DateTime, position.Time, symbol, side, PositionResult.Lose));
                            Lose++;
                            Money -= FeeSize;
                        }
                        else // 익절하고 손절
                        {
                            Money += position.StopLossPrice * position.Quantity;
                            Positions.Remove(position);
                            PositionHistories.Add(new PositionHistory(c0.DateTime, position.Time, symbol, side, PositionResult.LittleWin));
                            Win++;
                            Money -= FeeSize;
                        }
                    }
                    // 1차 익절
                    else if (position.Stage == 0 && c0.Quote.Close <= position.TakeProfitPrice)
                    {
                        var quantity = position.Quantity / 2;
                        Money += position.TakeProfitPrice * quantity;
                        position.Quantity -= quantity;
                        position.Stage = 1;

                        // TP/SL 재설정
                        var stopLossPrice = position.EntryPrice;
                        var takeProfitPrice = c0.Quote.Close + c0.Quote.Close - stopLossPrice;
                        position.StopLossPrice = stopLossPrice;
                        position.TakeProfitPrice = takeProfitPrice;
                    }
                    // 2차 익절
                    else if (position.Stage == 1 && c0.Quote.Close <= position.TakeProfitPrice)
                    {
                        Money += position.TakeProfitPrice * position.Quantity;

                        Positions.Remove(position);
                        PositionHistories.Add(new PositionHistory(c0.DateTime, position.Time, symbol, side, PositionResult.Win));
                        Win++;
                        Money -= FeeSize;
                    }
                }
            }
        }

        public void EvaluateTsLongNextCandle()
        {
            var side = PositionSide.Long;
            foreach (var symbol in MonitoringSymbols)
            {
                var position = Positions.Find(x => x.Symbol.Equals(symbol) && x.Side.Equals(side));

                var charts = Charts[symbol];
                var c0 = charts[^1];
                var c1 = charts[^2];
                var c2 = charts[^3];

                // 포지션이 없으면
                if (position == null)
                {
                    if (LongPositionCount >= MaxActiveDeals)
                    {
                        continue;
                    }

                    // TS 연두색 선이 2개 이상 존재하고 EMA 200 선 위에 있고 StochRSI K값이 20 골든크로스 하면
                    if (IsTwoGreenSignal(c1) && (double)c1.Quote.Close > c1.Ema1 && c1.K > 20 && c2.K < 20)
                    {
                        var price = c0.Quote.Open;
                        var stopLossPrice = (decimal)MinGreenSignal(c1);
                        var takeProfitPrice = 3 * price - 2 * stopLossPrice;
                        var quantity = BaseOrderSize / price;
                        Money -= price * quantity;
                        var newPosition = new Position(c0.DateTime, symbol, side, price)
                        {
                            StopLossPrice = stopLossPrice,
                            TakeProfitPrice = takeProfitPrice,
                            Quantity = quantity
                        };
                        Positions.Add(newPosition);
                    }
                }
                // 포지션이 있으면
                else
                {
                    // 손절
                    if (c0.Quote.Low <= position.StopLossPrice)
                    {
                        //if (position.TakeProfitCount == 0) // 익절없이 손절
                        {
                            Money += position.StopLossPrice * position.Quantity;
                            Positions.Remove(position);
                            PositionHistories.Add(new PositionHistory(c0.DateTime, position.Time, symbol, side, PositionResult.Lose));
                            Lose++;
                            Money -= FeeSize;
                        }
                        //else // 익절하고 손절
                        //{
                        //    Money += position.StopLossPrice * position.Quantity;
                        //    Positions.Remove(position);
                        //    PositionHistories.Add(new PositionHistory(c0.DateTime, position.Time, symbol, side, PositionResult.LittleWin));
                        //    Win++;
                        //    Money -= FeeSize;
                        //}
                    }
                    // 1차 익절
                    else if (position.Stage == 0 && c0.Quote.High >= position.TakeProfitPrice)
                    {
                        Money += position.TakeProfitPrice * position.Quantity;

                        Positions.Remove(position);
                        PositionHistories.Add(new PositionHistory(c0.DateTime, position.Time, symbol, side, PositionResult.Win));
                        Win++;
                        Money -= FeeSize;

                        //var quantity = position.Quantity / 2;
                        //Money += position.TakeProfitPrice * quantity;
                        //position.Quantity -= quantity;
                        //position.TakeProfitCount = 1;

                        //// TP/SL 재설정
                        //var stopLossPrice = position.EntryPrice;
                        //var takeProfitPrice = 2 * position.TakeProfitPrice - 1 * stopLossPrice;
                        //position.StopLossPrice = stopLossPrice;
                        //position.TakeProfitPrice = takeProfitPrice;
                    }

                    // 2차 익절
                    if (position.Stage == 1 && c0.Quote.High >= position.TakeProfitPrice)
                    {
                        Money += position.TakeProfitPrice * position.Quantity;

                        Positions.Remove(position);
                        PositionHistories.Add(new PositionHistory(c0.DateTime, position.Time, symbol, side, PositionResult.Win));
                        Win++;
                        Money -= FeeSize;
                    }
                }
            }
        }

        public void EvaluateTsShortNextCandle()
        {
            var side = PositionSide.Short;
            foreach (var symbol in MonitoringSymbols)
            {
                var position = Positions.Find(x => x.Symbol.Equals(symbol) && x.Side.Equals(side));

                var charts = Charts[symbol];
                var c0 = charts[^1];
                var c1 = charts[^2];
                var c2 = charts[^3];

                // 포지션이 없으면
                if (position == null)
                {
                    if (ShortPositionCount >= MaxActiveDeals)
                    {
                        continue;
                    }

                    // TS 빨간색 선이 2개 이상 존재하고 EMA 200 선 아래에 있고 StochRSI K값이 80 데드크로스 하면
                    if (IsTwoRedSignal(c1) && (double)c1.Quote.Close < c1.Ema1 && c1.K < 80 && c2.K > 80)
                    {
                        var price = c0.Quote.Open;
                        var stopLossPrice = (decimal)MaxRedSignal(c1);
                        var takeProfitPrice = 3 * price - 2 * stopLossPrice;
                        var quantity = BaseOrderSize / price;
                        Money -= price * quantity;
                        var newPosition = new Position(c0.DateTime, symbol, side, price)
                        {
                            StopLossPrice = stopLossPrice,
                            TakeProfitPrice = takeProfitPrice,
                            Quantity = quantity
                        };
                        Positions.Add(newPosition);

                        break;
                    }
                }
                // 포지션이 있으면
                else
                {
                    // 손절
                    if (c0.Quote.High >= position.StopLossPrice)
                    {
                        //if (position.TakeProfitCount == 0) // 익절없이 손절
                        {
                            Money += position.StopLossPrice * position.Quantity;
                            Positions.Remove(position);
                            PositionHistories.Add(new PositionHistory(c0.DateTime, position.Time, symbol, side, PositionResult.Lose));
                            Lose++;
                            Money -= FeeSize;
                        }
                        //else // 익절하고 손절
                        //{
                        //    Money += position.StopLossPrice * position.Quantity;
                        //    Positions.Remove(position);
                        //    PositionHistories.Add(new PositionHistory(c0.DateTime, position.Time, symbol, side, PositionResult.LittleWin));
                        //    Win++;
                        //    Money -= FeeSize;
                        //}
                    }
                    // 1차 익절
                    else if (position.Stage == 0 && c0.Quote.Low <= position.TakeProfitPrice)
                    {
                        Money += position.TakeProfitPrice * position.Quantity;

                        Positions.Remove(position);
                        PositionHistories.Add(new PositionHistory(c0.DateTime, position.Time, symbol, side, PositionResult.Win));
                        Win++;
                        Money -= FeeSize;

                        //var quantity = position.Quantity / 2;
                        //Money += position.TakeProfitPrice * quantity;
                        //position.Quantity -= quantity;
                        //position.TakeProfitCount = 1;

                        //// TP/SL 재설정
                        //var stopLossPrice = position.EntryPrice;
                        //var takeProfitPrice = 2 * position.TakeProfitPrice - 1 * stopLossPrice;
                        //position.StopLossPrice = stopLossPrice;
                        //position.TakeProfitPrice = takeProfitPrice;
                    }

                    // 2차 익절
                    if (position.Stage == 1 && c0.Quote.Low <= position.TakeProfitPrice)
                    {
                        Money += position.TakeProfitPrice * position.Quantity;

                        Positions.Remove(position);
                        PositionHistories.Add(new PositionHistory(c0.DateTime, position.Time, symbol, side, PositionResult.Win));
                        Win++;
                        Money -= FeeSize;
                    }
                }
            }
        }
        #endregion

        #region TS2
        public void EvaluateTs2LongNextCandle()
        {
            var side = PositionSide.Long;
            foreach (var symbol in MonitoringSymbols)
            {
                var position = Positions.Find(x => x.Symbol.Equals(symbol) && x.Side.Equals(side));

                var charts = Charts[symbol];
                var c0 = charts[^1];
                var c1 = charts[^2];
                var c2 = charts[^3];

                // 포지션이 없으면
                if (position == null)
                {
                    if (LongPositionCount >= MaxActiveDeals)
                    {
                        continue;
                    }

                    if (c1.Supertrend1 > 0 && c2.Supertrend1 < 0 && c1.Supertrend2 > 0 && c1.Supertrend3 > 0)
                    {
                        var price = c0.Quote.Open;
                        var stopLossPrice = (decimal)Math.Abs(c1.Supertrend2);
                        var takeProfitPrice = 2 * price - 1 * stopLossPrice;
                        var quantity = BaseOrderSize / price;
                        Money -= price * quantity;
                        var newPosition = new Position(c0.DateTime, symbol, side, price)
                        {
                            TakeProfitPrice = takeProfitPrice,
                            Quantity = quantity,
                            EntryAmount = price * quantity
                        };
                        Positions.Add(newPosition);
                    }
                }
                // 포지션이 있으면
                else
                {
                    var st2 = (decimal)Math.Abs(c0.Supertrend2);
                    var dst2 = Calculator.TargetPrice(side, st2, -0.8m);

                    // 전량 정리
                    if (c1.Supertrend2 < 0)
                    {
                        var price = c0.Quote.Open;
                        var quantity = position.Quantity;
                        Money += price * quantity;
                        Positions.Remove(position);
                        PositionHistories.Add(new PositionHistory(c0.DateTime, position.Time, symbol, side, PositionResult.Lose)
                        {
                            EntryAmount = position.EntryAmount,
                            ExitAmount = price * quantity
                        });
                        Lose++;
                        Money -= FeeSize;
                    }
                    //else if (position.Stage == 0 && c0.Quote.Low <= dst2)
                    //{
                    //    var price = dst2;
                    //    var quantity = position.Quantity;
                    //    Money += price * quantity;
                    //    Positions.Remove(position);
                    //    PositionHistories.Add(new PositionHistory(c0.DateTime, position.Time, symbol, side, PositionResult.Lose)
                    //    {
                    //        EntryAmount = position.EntryAmount,
                    //        ExitAmount = price * quantity
                    //    });
                    //    Lose++;
                    //    Money -= FeeSize;
                    //}
                    // 1차 익절
                    else if (position.Stage == 0 && c0.Quote.High >= position.TakeProfitPrice)
                    {
                        var price = position.TakeProfitPrice;
                        var quantity = position.Quantity / 2;
                        Money += price * quantity;
                        position.Quantity -= quantity;
                        position.ExitAmount = price * quantity;
                        position.Stage = 1;
                    }
                    // 2차 정리
                    //else if (position.Stage == 1 && c1.Supertrend1 < 0)
                    //{
                    //    var price = c0.Quote.Open;
                    //    var quantity = position.Quantity;
                    //    Money += price * quantity;
                    //    Positions.Remove(position);
                    //    PositionHistories.Add(new PositionHistory(c0.DateTime, position.Time, symbol, side, PositionResult.Win)
                    //    {
                    //        EntryAmount = position.EntryAmount,
                    //        ExitAmount = position.ExitAmount + price * quantity
                    //    });
                    //    Win++;
                    //    Money -= FeeSize;
                    //}
                }
            }
        }

        public void EvaluateTs2ShortNextCandle()
        {
            var side = PositionSide.Short;
            foreach (var symbol in MonitoringSymbols)
            {
                var position = Positions.Find(x => x.Symbol.Equals(symbol) && x.Side.Equals(side));

                var charts = Charts[symbol];
                var c0 = charts[^1];
                var c1 = charts[^2];
                var c2 = charts[^3];

                // 포지션이 없으면
                if (position == null)
                {
                    if (ShortPositionCount >= MaxActiveDeals)
                    {
                        continue;
                    }

                    if (c1.Supertrend1 < 0 && c2.Supertrend1 > 0 && c1.Supertrend2 < 0 && c1.Supertrend3 < 0)
                    {
                        var price = c0.Quote.Open;
                        var stopLossPrice = (decimal)Math.Abs(c1.Supertrend2);
                        var takeProfitPrice = 2 * price - 1 * stopLossPrice;
                        var quantity = BaseOrderSize / price;
                        Money += price * quantity;
                        var newPosition = new Position(c0.DateTime, symbol, side, price)
                        {
                            TakeProfitPrice = takeProfitPrice,
                            Quantity = quantity,
                            EntryAmount = price * quantity
                        };
                        Positions.Add(newPosition);
                    }
                }
                // 포지션이 있으면
                else
                {
                    var st2 = (decimal)Math.Abs(c0.Supertrend2);
                    var dst2 = Calculator.TargetPrice(side, st2, -0.8m);

                    // 전량 정리
                    if (c1.Supertrend2 > 0)
                    {
                        var price = c0.Quote.Open;
                        var quantity = position.Quantity;
                        Money -= price * quantity;
                        Positions.Remove(position);
                        PositionHistories.Add(new PositionHistory(c0.DateTime, position.Time, symbol, side, PositionResult.Lose)
                        {
                            EntryAmount = position.EntryAmount,
                            ExitAmount = price * quantity
                        });
                        Lose++;
                        Money -= FeeSize;
                    }
                    //else if (position.Stage == 0 && c0.Quote.High >= dst2)
                    //{
                    //    var price = dst2;
                    //    var quantity = position.Quantity;
                    //    Money -= price * quantity;
                    //    Positions.Remove(position);
                    //    PositionHistories.Add(new PositionHistory(c0.DateTime, position.Time, symbol, side, PositionResult.Lose)
                    //    {
                    //        EntryAmount = position.EntryAmount,
                    //        ExitAmount = price * quantity
                    //    });
                    //    Lose++;
                    //    Money -= FeeSize;
                    //}
                    // 1차 익절
                    else if (position.Stage == 0 && c0.Quote.Low <= position.TakeProfitPrice)
                    {
                        var price = position.TakeProfitPrice;
                        var quantity = position.Quantity / 2;
                        Money -= price * quantity;
                        position.Quantity -= quantity;
                        position.ExitAmount = price * quantity;
                        position.Stage = 1;
                    }
                    // 2차 정리
                    //else if (position.Stage == 1 && c1.Supertrend1 > 0)
                    //{
                    //    var price = c0.Quote.Open;
                    //    var quantity = position.Quantity;
                    //    Money -= price * quantity;
                    //    Positions.Remove(position);
                    //    PositionHistories.Add(new PositionHistory(c0.DateTime, position.Time, symbol, side, PositionResult.Win)
                    //    {
                    //        EntryAmount = position.EntryAmount,
                    //        ExitAmount = position.ExitAmount + price * quantity
                    //    });
                    //    Win++;
                    //    Money -= FeeSize;
                    //}
                }
            }
        }

        public void EvaluateTs2LongSimple()
        {
            var side = PositionSide.Long;
            foreach (var symbol in MonitoringSymbols)
            {
                var position = Positions.Find(x => x.Symbol.Equals(symbol) && x.Side.Equals(side));

                var charts = Charts[symbol];
                var c0 = charts[^1];
                var c1 = charts[^2];
                var c2 = charts[^3];

                // 포지션이 없으면
                if (position == null)
                {
                    if (LongPositionCount >= MaxActiveDeals)
                    {
                        continue;
                    }

                    if (c1.Supertrend1 > 0 && c2.Supertrend1 < 0 && c1.Supertrend2 > 0 && c1.Supertrend3 > 0)
                    {
                        var price = c0.Quote.Open;
                        var stopLossPrice = (decimal)Math.Abs(c1.Supertrend2);
                        var takeProfitPrice = 2 * price - 1 * stopLossPrice;
                        var quantity = BaseOrderSize / price;
                        Money -= price * quantity;
                        var newPosition = new Position(c0.DateTime, symbol, side, price)
                        {
                            EntryPrice = price,
                            TakeProfitPrice = takeProfitPrice,
                            Quantity = quantity,
                            EntryAmount = price * quantity
                        };
                        Positions.Add(newPosition);
                    }
                }
                // 포지션이 있으면
                else
                {
                    var per = 0m;
                    var testPrice = c0.Quote.Open;
                    while (testPrice >= c0.Quote.Low)
                    {
                        var quote = new Quote
                        {
                            Date = c0.DateTime,
                            Open = c0.Quote.Open,
                            High = c0.Quote.High,
                            Low = c0.Quote.Low,
                            Close = testPrice,
                            Volume = c0.Quote.Volume
                        };
                        var newCharts = FastCalculateIndicatorsTs2(symbol, quote);
                        var nc0 = newCharts[^1];
                        if (nc0.Supertrend1 < 0)
                        {
                            var price = testPrice;
                            var quantity = position.Quantity;
                            Money += price * quantity;
                            Positions.Remove(position);
                            PositionHistories.Add(new PositionHistory(c0.DateTime, position.Time, symbol, side, PositionResult.Win)
                            {
                                EntryPrice = position.EntryPrice,
                                ExitPrice = price,
                                EntryAmount = position.EntryAmount,
                                ExitAmount = position.ExitAmount + price * quantity
                            });
                            Win++;
                            Money -= FeeSize;
                            break;
                        }

                        per -= 0.05m;
                        testPrice = Calculator.TargetPrice(side, c0.Quote.Open, per);
                    }

                    //if (c0.Supertrend1 < 0)
                    //{
                    //    var price = c0.Quote.Close;
                    //    var quantity = position.Quantity;
                    //    Money += price * quantity;
                    //    Positions.Remove(position);
                    //    PositionHistories.Add(new PositionHistory(c0.DateTime, position.Time, symbol, side, PositionResult.Win)
                    //    {
                    //        EntryPrice = position.EntryPrice,
                    //        ExitPrice = price,
                    //        EntryAmount = position.EntryAmount,
                    //        ExitAmount = position.ExitAmount + price * quantity
                    //    });
                    //    Win++;
                    //    Money -= FeeSize;
                    //}
                }
            }
        }

        public void EvaluateTs2ShortSimple()
        {
            var side = PositionSide.Short;
            foreach (var symbol in MonitoringSymbols)
            {
                var position = Positions.Find(x => x.Symbol.Equals(symbol) && x.Side.Equals(side));

                var charts = Charts[symbol];
                var c0 = charts[^1];
                var c1 = charts[^2];
                var c2 = charts[^3];

                // 포지션이 없으면
                if (position == null)
                {
                    if (ShortPositionCount >= MaxActiveDeals)
                    {
                        continue;
                    }

                    if (c1.Supertrend1 < 0 && c2.Supertrend1 > 0 && c1.Supertrend2 < 0 && c1.Supertrend3 < 0)
                    {
                        var price = c0.Quote.Open;
                        var stopLossPrice = (decimal)Math.Abs(c1.Supertrend2);
                        var takeProfitPrice = 2 * price - 1 * stopLossPrice;
                        var quantity = BaseOrderSize / price;
                        Money += price * quantity;
                        var newPosition = new Position(c0.DateTime, symbol, side, price)
                        {
                            EntryPrice = price,
                            TakeProfitPrice = takeProfitPrice,
                            Quantity = quantity,
                            EntryAmount = price * quantity
                        };
                        Positions.Add(newPosition);
                    }
                }
                // 포지션이 있으면
                else
                {
                    var per = 0m;
                    var testPrice = c0.Quote.Open;
                    while (testPrice <= c0.Quote.High)
                    {
                        var quote = new Quote
                        {
                            Date = c0.DateTime,
                            Open = c0.Quote.Open,
                            High = c0.Quote.High,
                            Low = c0.Quote.Low,
                            Close = testPrice,
                            Volume = c0.Quote.Volume
                        };
                        var newCharts = FastCalculateIndicatorsTs2(symbol, quote);
                        var nc0 = newCharts[^1];
                        if (nc0.Supertrend1 > 0)
                        {
                            var price = testPrice;
                            var quantity = position.Quantity;
                            Money -= price * quantity;
                            Positions.Remove(position);
                            PositionHistories.Add(new PositionHistory(c0.DateTime, position.Time, symbol, side, PositionResult.Win)
                            {
                                EntryPrice = position.EntryPrice,
                                ExitPrice = price,
                                EntryAmount = position.EntryAmount,
                                ExitAmount = position.ExitAmount + price * quantity
                            });
                            Win++;
                            Money -= FeeSize;
                            break;
                        }

                        per -= 0.05m;
                        testPrice = Calculator.TargetPrice(side, c0.Quote.Open, per);
                    }

                    //if (c0.Supertrend1 > 0)
                    //{
                    //    var price = c0.Quote.Close;
                    //    var quantity = position.Quantity;
                    //    Money -= price * quantity;
                    //    Positions.Remove(position);
                    //    PositionHistories.Add(new PositionHistory(c0.DateTime, position.Time, symbol, side, PositionResult.Win)
                    //    {
                    //        EntryPrice = position.EntryPrice,
                    //        ExitPrice = price,
                    //        EntryAmount = position.EntryAmount,
                    //        ExitAmount = position.ExitAmount + price * quantity
                    //    });
                    //    Win++;
                    //    Money -= FeeSize;
                    //}
                }
            }
        }

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
        private bool IsEntryTs2ShortBit(List<ChartInfo> charts)
        {
            int condition = 0;
            for (int i = charts.Count - 2; i >= 0; i--) // 이전 봉 기준
            {
                var chart = charts[i];

                switch (condition)
                {
                    case 0:
                        if (chart.Supertrend1 < 0 && chart.Supertrend2 < 0 && chart.Supertrend3 < 0)
                        {
                            condition = 1;
                        }
                        else
                        {
                            return false;
                        }
                        break;
                    case 1:
                        if (chart.Supertrend1 > 0 && chart.Supertrend2 < 0 && chart.Supertrend3 < 0)
                        {
                            condition = 2;
                        }
                        else
                        {
                            return false;
                        }
                        break;
                    case 2:
                        if (chart.Supertrend1 > 0 && chart.Supertrend2 < 0 && chart.Supertrend3 < 0)
                        {

                        }
                        else if (chart.Supertrend1 < 0 && chart.Supertrend2 < 0 && chart.Supertrend3 < 0)
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

        public void EvaluateTs2LongBit()
        {
            var side = PositionSide.Long;
            foreach (var symbol in MonitoringSymbols)
            {
                var position = Positions.Find(x => x.Symbol.Equals(symbol) && x.Side.Equals(side));

                var charts = Charts[symbol];
                var c0 = charts[^1];
                var c1 = charts[^2];

                // 포지션이 없으면
                if (position == null)
                {
                    if (LongPositionCount >= MaxActiveDeals)
                    {
                        continue;
                    }

                    if (IsEntryTs2LongBit(charts))
                    {
                        var price = c0.Quote.Open;
                        var stopLossPrice = (decimal)Math.Abs(c1.Supertrend2);
                        var takeProfitPrice = 2 * price - 1 * stopLossPrice;
                        var quantity = BaseOrderSize / price;
                        Money -= price * quantity;
                        var newPosition = new Position(c0.DateTime, symbol, side, price)
                        {
                            StopLossPrice = stopLossPrice,
                            TakeProfitPrice = takeProfitPrice,
                            Quantity = quantity,
                            EntryAmount = price * quantity
                        };
                        Positions.Add(newPosition);
                    }
                }
                // 포지션이 있으면
                else
                {
                    // 2차 익절
                    if (position.Stage == 1 && c1.Supertrend1 < 0)
                    {
                        var price = c0.Quote.Open;
                        var quantity = position.Quantity;
                        Money += price * quantity;
                        Positions.Remove(position);
                        PositionHistories.Add(new PositionHistory(c0.DateTime, position.Time, symbol, side, PositionResult.Win)
                        {
                            EntryAmount = position.EntryAmount,
                            ExitAmount = position.ExitAmount + price * quantity
                        });
                        Win++;
                        Money -= FeeSize;
                    }
                    // 1차 익절
                    else if (position.Stage == 0 && c0.Quote.High >= position.TakeProfitPrice)
                    {
                        var price = position.TakeProfitPrice;
                        var quantity = position.Quantity / 2;
                        Money += price * quantity;
                        position.Quantity -= quantity;
                        position.ExitAmount = price * quantity;
                        position.Stage = 1;
                    }
                    // 손절
                    else if (c0.Quote.Low <= position.StopLossPrice)
                    {
                        var price = position.StopLossPrice;
                        var quantity = position.Quantity;
                        Money += price * quantity;
                        Positions.Remove(position);
                        PositionHistories.Add(new PositionHistory(c0.DateTime, position.Time, symbol, side, PositionResult.Lose)
                        {
                            EntryAmount = position.EntryAmount,
                            ExitAmount = price * quantity
                        });
                        Lose++;
                        Money -= FeeSize;
                    }
                }
            }
        }

        public void EvaluateTs2ShortBit()
        {
            var side = PositionSide.Short;
            foreach (var symbol in MonitoringSymbols)
            {
                var position = Positions.Find(x => x.Symbol.Equals(symbol) && x.Side.Equals(side));

                var charts = Charts[symbol];
                var c0 = charts[^1];
                var c1 = charts[^2];

                // 포지션이 없으면
                if (position == null)
                {
                    if (ShortPositionCount >= MaxActiveDeals)
                    {
                        continue;
                    }

                    if (IsEntryTs2ShortBit(charts))
                    {
                        var price = c0.Quote.Open;
                        var stopLossPrice = (decimal)Math.Abs(c1.Supertrend2);
                        var takeProfitPrice = 2 * price - 1 * stopLossPrice;
                        var quantity = BaseOrderSize / price;
                        Money += price * quantity;
                        var newPosition = new Position(c0.DateTime, symbol, side, price)
                        {
                            StopLossPrice = stopLossPrice,
                            TakeProfitPrice = takeProfitPrice,
                            Quantity = quantity,
                            EntryAmount = price * quantity
                        };
                        Positions.Add(newPosition);
                    }
                }
                // 포지션이 있으면
                else
                {
                    // 2차 익절
                    if (position.Stage == 1 && c1.Supertrend1 > 0)
                    {
                        var price = c0.Quote.Open;
                        var quantity = position.Quantity;
                        Money -= price * quantity;
                        Positions.Remove(position);
                        PositionHistories.Add(new PositionHistory(c0.DateTime, position.Time, symbol, side, PositionResult.Win)
                        {
                            EntryAmount = position.EntryAmount,
                            ExitAmount = position.ExitAmount + price * quantity
                        });
                        Win++;
                        Money -= FeeSize;
                    }
                    // 1차 익절
                    else if (position.Stage == 0 && c0.Quote.Low <= position.TakeProfitPrice)
                    {
                        var price = position.TakeProfitPrice;
                        var quantity = position.Quantity / 2;
                        Money -= price * quantity;
                        position.Quantity -= quantity;
                        position.ExitAmount = price * quantity;
                        position.Stage = 1;
                    }
                    // 손절
                    else if (c0.Quote.High >= position.StopLossPrice)
                    {
                        var price = position.StopLossPrice;
                        var quantity = position.Quantity;
                        Money -= price * quantity;
                        Positions.Remove(position);
                        PositionHistories.Add(new PositionHistory(c0.DateTime, position.Time, symbol, side, PositionResult.Lose)
                        {
                            EntryAmount = position.EntryAmount,
                            ExitAmount = price * quantity
                        });
                        Lose++;
                        Money -= FeeSize;
                    }
                }
            }
        }
        #endregion

        #region MACD
        public void CalculateIndicatorsMacd()
        {
            foreach (var chart in Charts)
            {
                var quotes = chart.Value.Select(x => x.Quote);
                var macd = quotes.GetMacd(12, 26, 9);
                var m = macd.Select(x => x.Macd);
                var s = macd.Select(x => x.Signal);
                var st = quotes.GetSupertrend(10, 3).Select(x=>x.Supertrend);
                var adx = quotes.GetAdx(14, 14).Select(x => x.Adx);
                for (int i = 0; i < chart.Value.Count; i++)
                {
                    var _chart = chart.Value[i];
                    _chart.Macd = m.ElementAt(i);
                    _chart.MacdSignal = s.ElementAt(i);
                    _chart.Supertrend1 = st.ElementAt(i);
                    _chart.Adx = adx.ElementAt(i);
                }
            }
        }

        /// <summary>
        /// 진입 : MACD가 Signal을 음의 자리에서 골든크로스, ADX가 30이상, 너무 길지 않은 양봉(이더기준 0.5% 미만)
        /// 손절 : 이전 저점(이전10봉), 즉시
        /// 1차 익절 : 손절비 1:1, 즉시
        /// 2차 정리 : 슈퍼트렌드(10,3) < 0, 즉시
        /// </summary>
        public void EvaluateMacdLongNextCandle()
        {
            var side = PositionSide.Long;
            foreach (var symbol in MonitoringSymbols)
            {
                var position = Positions.Find(x => x.Symbol.Equals(symbol) && x.Side.Equals(side));

                var charts = Charts[symbol];
                var c0 = charts[^1];
                var c1 = charts[^2];
                var c2 = charts[^3];
                var bottomPrice = charts.TakeLast(31).Min(x => x.Quote.Low);
                var c1CandleLength = Math.Abs(Calculator.Roe(side, c1.Quote.Open, c1.Quote.Close));
                var bottomLength = Calculator.Roe(side, c0.Quote.Open, bottomPrice);

                if (position == null)
                {
                    if (LongPositionCount >= MaxActiveDeals)
                    {
                        continue;
                    }

                    if (c1.Adx > 30 && c1.Macd < 0 && c1.Macd > c1.MacdSignal && c2.Macd < c2.MacdSignal &&
                        c1CandleLength < 0.5m &&
                        bottomLength > -10)
                    {
                        var price = c0.Quote.Open;
                        var stopLossPrice = bottomPrice;
                        var takeProfitPrice = 2 * price - 1 * stopLossPrice;
                        var quantity = BaseOrderSize / price;
                        Money -= price * quantity;
                        var newPosition = new Position(c0.DateTime, symbol, side, price)
                        {
                            TakeProfitPrice = takeProfitPrice,
                            StopLossPrice = stopLossPrice,
                            Quantity = quantity,
                            EntryAmount = price * quantity
                        };
                        Positions.Add(newPosition);
                    }
                }
                else
                {
                    if (position.Stage == 0 && c0.Quote.Low <= position.StopLossPrice)
                    {
                        var price = position.StopLossPrice;
                        var quantity = position.Quantity;
                        Money += price * quantity;
                        Positions.Remove(position);
                        PositionHistories.Add(new PositionHistory(c0.DateTime, position.Time, symbol, side, PositionResult.Lose)
                        {
                            EntryAmount = position.EntryAmount,
                            ExitAmount = price * quantity
                        });
                        Lose++;
                        Money -= FeeSize;
                    }
                    else if (position.Stage == 0 && c0.Quote.High >= position.TakeProfitPrice)
                    {
                        var price = position.TakeProfitPrice;
                        var quantity = position.Quantity / 2;
                        Money += price * quantity;
                        position.Quantity -= quantity;
                        position.ExitAmount = price * quantity;
                        position.Stage = 1;
                    }
                    else if(position.Stage == 1 && c0.Supertrend1 < 0)
                    {
                        var price = (decimal)Math.Abs(c1.Supertrend1);
                        var quantity = position.Quantity;
                        Money += price * quantity;
                        Positions.Remove(position);
                        PositionHistories.Add(new PositionHistory(c0.DateTime, position.Time, symbol, side, PositionResult.Win)
                        {
                            EntryAmount = position.EntryAmount,
                            ExitAmount = position.ExitAmount + price * quantity
                        });
                        Win++;
                        Money -= FeeSize;
                    }
                }
            }
        }
        
        public void EvaluateMacdShortNextCandle()
        {
            var side = PositionSide.Short;
            foreach (var symbol in MonitoringSymbols)
            {
                var position = Positions.Find(x => x.Symbol.Equals(symbol) && x.Side.Equals(side));

                var charts = Charts[symbol];
                var c0 = charts[^1];
                var c1 = charts[^2];
                var c2 = charts[^3];
                var topPrice = charts.TakeLast(31).Max(x => x.Quote.High);
                var c1CandleLength = Math.Abs(Calculator.Roe(side, c1.Quote.Open, c1.Quote.Close));
                var topLength = Calculator.Roe(side, c0.Quote.Open, topPrice);

                if (position == null)
                {
                    if (ShortPositionCount >= MaxActiveDeals)
                    {
                        continue;
                    }

                    if (c1.Adx > 30 && c1.Macd > 0 && c1.Macd < c1.MacdSignal && c2.Macd > c2.MacdSignal &&
                        c1CandleLength < 0.5m &&
                        topLength > -10)
                    {
                        var price = c0.Quote.Open;
                        var stopLossPrice = topPrice;
                        var takeProfitPrice = 2 * price - 1 * stopLossPrice;
                        var quantity = BaseOrderSize / price;
                        Money += price * quantity;
                        var newPosition = new Position(c0.DateTime, symbol, side, price)
                        {
                            StopLossPrice = stopLossPrice,
                            TakeProfitPrice = takeProfitPrice,
                            Quantity = quantity,
                            EntryAmount = price * quantity
                        };
                        Positions.Add(newPosition);
                    }
                }
                else
                {
                    if (position.Stage == 0 && c0.Quote.High >= position.StopLossPrice)
                    {
                        var price = position.StopLossPrice;
                        var quantity = position.Quantity;
                        Money -= price * quantity;
                        Positions.Remove(position);
                        PositionHistories.Add(new PositionHistory(c0.DateTime, position.Time, symbol, side, PositionResult.Lose)
                        {
                            EntryAmount = position.EntryAmount,
                            ExitAmount = price * quantity
                        });
                        Lose++;
                        Money -= FeeSize;
                    }
                    else if (position.Stage == 0 && c0.Quote.Low <= position.TakeProfitPrice)
                    {
                        var price = position.TakeProfitPrice;
                        var quantity = position.Quantity / 2;
                        Money -= price * quantity;
                        position.Quantity -= quantity;
                        position.ExitAmount = price * quantity;
                        position.Stage = 1;
                    }
                    else if (position.Stage == 1 && c0.Supertrend1 > 0)
                    {
                        var price = (decimal)Math.Abs(c1.Supertrend1);
                        var quantity = position.Quantity;
                        Money -= price * quantity;
                        Positions.Remove(position);
                        PositionHistories.Add(new PositionHistory(c0.DateTime, position.Time, symbol, side, PositionResult.Win)
                        {
                            EntryAmount = position.EntryAmount,
                            ExitAmount = position.ExitAmount + price * quantity
                        });
                        Win++;
                        Money -= FeeSize;
                    }
                }
            }
        }
        #endregion
    }
}
