using Binance.Net.Enums;

using Skender.Stock.Indicators;

namespace CryptoModel.Charts
{
    public class ChartLoader
    {
        public static List<ChartPack> Charts { get; set; } = new();
        public static ChartPack GetChartPack(string symbol, KlineInterval interval) => Charts.Find(x => x.Symbol.Equals(symbol) && x.Interval.Equals(interval)) ?? default!;

        /// <summary>
        /// 분봉 초기화
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="interval"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        public static void InitChartsByDate(string symbol, KlineInterval interval, DateTime startDate, DateTime endDate)
        {
            try
            {
                var chartPack = new ChartPack(interval);

                switch (interval)
                {
                    case KlineInterval.OneMinute:
                    case KlineInterval.ThreeMinutes:
                    case KlineInterval.FiveMinutes:
                    case KlineInterval.FifteenMinutes:
                    case KlineInterval.ThirtyMinutes:
                    case KlineInterval.OneHour:
                    case KlineInterval.TwoHour:
                    case KlineInterval.FourHour:
                    case KlineInterval.SixHour:
                    case KlineInterval.EightHour:
                    case KlineInterval.TwelveHour:
                        var dayCount = (int)(endDate - startDate).TotalDays + 1;

                        for (int i = 0; i < dayCount; i++)
                        {
                            var _currentDate = startDate.AddDays(i);
                            var fileName = CryptoPath.BinanceFuturesData.Down("1m", symbol, $"{symbol}_{_currentDate:yyyy-MM-dd}.csv");
                            var data = File.ReadAllLines(fileName);

                            foreach (var d in data)
                            {
                                var e = d.Split(',');
                                var quote = new Quote
                                {
                                    Date = e[0].ToDateTime(),
                                    Open = e[1].ToDecimal(),
                                    High = e[2].ToDecimal(),
                                    Low = e[3].ToDecimal(),
                                    Close = e[4].ToDecimal(),
                                    Volume = e[5].ToDecimal()
                                };
                                chartPack.AddChart(new ChartInfo(symbol, quote));
                            }
                        }
                        break;

                    case KlineInterval.OneDay:
                    case KlineInterval.ThreeDay:
                    case KlineInterval.OneWeek:
                    case KlineInterval.OneMonth:
                        var path = CryptoPath.BinanceFuturesData.Down("1D", $"{symbol}.csv");
                        var data1 = File.ReadAllLines(path);

                        foreach (var d in data1)
                        {
                            var e = d.Split(',');
                            var quote = new Quote
                            {
                                Date = e[0].ToDateTime(),
                                Open = e[1].ToDecimal(),
                                High = e[2].ToDecimal(),
                                Low = e[3].ToDecimal(),
                                Close = e[4].ToDecimal(),
                                Volume = e[5].ToDecimal()
                            };
                            chartPack.AddChart(new ChartInfo(symbol, quote));
                        }
                        break;

                    default:
                        break;
                }

                chartPack.ConvertCandle();

                Charts.Add(chartPack);
            }
            catch (FileNotFoundException)
            {
                throw;
            }
        }
    }
}
