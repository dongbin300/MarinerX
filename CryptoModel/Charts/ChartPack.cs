﻿using Binance.Net.Enums;

namespace CryptoModel.Charts
{
    public class ChartPack
    {
        public string Symbol => Charts.First().Symbol;
        public KlineInterval Interval = KlineInterval.OneMinute;
        public IList<ChartInfo> Charts { get; set; } = new List<ChartInfo>();
        public DateTime StartTime => Charts.Min(x => x.DateTime);
        public DateTime EndTime => Charts.Max(x => x.DateTime);
        public ChartInfo? CurrentChart;

        public ChartPack(KlineInterval interval)
        {
            Interval = interval;
            CurrentChart = null;
        }

        public void AddChart(ChartInfo chart)
        {
            Charts.Add(chart);
        }

        public void ConvertCandle()
        {
            if (Interval == KlineInterval.OneMinute)
            {
                return;
            }

            var newQuotes = new List<Quote>();

            int unitCount = Interval switch
            {
                KlineInterval.ThreeMinutes => 3,
                KlineInterval.FiveMinutes => 5,
                KlineInterval.FifteenMinutes => 15,
                KlineInterval.ThirtyMinutes => 30,
                KlineInterval.OneHour => 60,
                KlineInterval.TwoHour => 120,
                KlineInterval.FourHour => 240,
                KlineInterval.SixHour => 360,
                KlineInterval.EightHour => 480,
                KlineInterval.TwelveHour => 720,
                KlineInterval.OneDay => 1440,
                _ => 1
            };

            int i = 0;
            for (; i < Charts.Count; i++)
            {
                if ((Charts[i].DateTime.Hour * 60 + Charts[i].DateTime.Minute) % unitCount == 0)
                {
                    break;
                }
            }

            for (; i < Charts.Count; i += unitCount)
            {
                var targets = Charts.Skip(i).Take(unitCount).Select(x => x.Quote).ToList();

                newQuotes.Add(new Quote
                {
                    Date = targets[0].Date,
                    Open = targets[0].Open,
                    High = targets.Max(t => t.High),
                    Low = targets.Min(t => t.Low),
                    Close = targets[^1].Close,
                    Volume = targets.Sum(t => t.Volume)
                });
            }

            var newChart = newQuotes.Select(candle => new ChartInfo(Symbol, candle)).ToList();
            Charts = newChart;
        }

        public void CalculateIndicatorsEveryonesCoin()
        {
            var quotes = Charts.Select(x => x.Quote);
            var r1 = quotes.GetLsma(10).Select(x => x.Lsma);
            var r2 = quotes.GetLsma(30).Select(x => x.Lsma);
            var r3 = quotes.GetRsi(14).Select(x => x.Rsi);
            for (int i = 0; i < Charts.Count; i++)
            {
                var chart = Charts[i];
                chart.Lsma1 = r1.ElementAt(i);
                chart.Lsma2 = r2.ElementAt(i);
                chart.Rsi = r3.ElementAt(i);
            }
        }

        public void CalculateIndicatorsStefano()
        {
            var quotes = Charts.Select(x => x.Quote);
            var r1 = quotes.GetEma(12).Select(x => x.Ema);
            var r2 = quotes.GetEma(26).Select(x => x.Ema);
            //var r3 = quotes.GetJmaSlope(14).Select(x => x.JmaSlope);
            for (int i = 0; i < Charts.Count; i++)
            {
                var chart = Charts[i];
                chart.Ema1 = r1.ElementAt(i);
                chart.Ema2 = r2.ElementAt(i);
                //chart.JmaSlope = r3.ElementAt(i);
            }
        }

        public ChartInfo Select()
        {
            return CurrentChart = GetChart(StartTime);
        }

        public ChartInfo Select(DateTime time)
        {
            return CurrentChart = GetChart(time);
        }

        public ChartInfo Next() =>
            CurrentChart == null ?
            CurrentChart = default! :
            CurrentChart = GetChart(Interval switch
            {
                KlineInterval.OneMinute => CurrentChart.DateTime.AddMinutes(1),
                KlineInterval.ThreeMinutes => CurrentChart.DateTime.AddMinutes(3),
                KlineInterval.FiveMinutes => CurrentChart.DateTime.AddMinutes(5),
                KlineInterval.FifteenMinutes => CurrentChart.DateTime.AddMinutes(15),
                KlineInterval.ThirtyMinutes => CurrentChart.DateTime.AddMinutes(30),
                KlineInterval.OneHour => CurrentChart.DateTime.AddHours(1),
                KlineInterval.TwoHour => CurrentChart.DateTime.AddHours(2),
                KlineInterval.FourHour => CurrentChart.DateTime.AddHours(4),
                KlineInterval.SixHour => CurrentChart.DateTime.AddHours(6),
                KlineInterval.EightHour => CurrentChart.DateTime.AddHours(8),
                KlineInterval.TwelveHour => CurrentChart.DateTime.AddHours(12),
                KlineInterval.OneDay => CurrentChart.DateTime.AddDays(1),
                _ => CurrentChart.DateTime.AddMinutes(1)
            });

        public ChartInfo GetChart(DateTime dateTime) => Charts.First(x => x.DateTime.Equals(dateTime));

        public List<ChartInfo> GetCharts(DateTime startTime, DateTime endTime)
        {
            return Charts.Where(x => x.DateTime >= startTime && x.DateTime <= endTime).ToList();
        }
    }
}
