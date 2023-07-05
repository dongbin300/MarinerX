using CryptoModel;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Media;

namespace MarinerX.Bot.Models
{
    public class PairQuote
    {
        private int DecimalCount = 4;

        public string Symbol { get; set; }
        public List<ChartInfo> Charts { get; set; }
        public decimal CurrentPrice => Charts[^1].Quote.Close;
        public double CurrentSupertrend1 => Math.Round(Charts[^1].Supertrend1, DecimalCount);
        public double CurrentSupertrend2 => Math.Round(Charts[^1].Supertrend2, DecimalCount);
        public double CurrentSupertrend3 => Math.Round(Charts[^1].Supertrend3, DecimalCount);
        public SolidColorBrush CurrentSupertrend1Color => CurrentSupertrend1 >= 0 ? Common.LongColor : Common.ShortColor;
        public SolidColorBrush CurrentSupertrend2Color => CurrentSupertrend2 >= 0 ? Common.LongColor : Common.ShortColor;
        public SolidColorBrush CurrentSupertrend3Color => CurrentSupertrend3 >= 0 ? Common.LongColor : Common.ShortColor;
        public double PrevSupertrend1 => Math.Round(Charts[^2].Supertrend1, DecimalCount);
        public double PrevSupertrend2 => Math.Round(Charts[^2].Supertrend2, DecimalCount);
        public double PrevSupertrend3 => Math.Round(Charts[^2].Supertrend3, DecimalCount);
        public SolidColorBrush PrevSupertrend1Color => PrevSupertrend1 >= 0 ? Common.LongColor : Common.ShortColor;
        public SolidColorBrush PrevSupertrend2Color => PrevSupertrend2 >= 0 ? Common.LongColor : Common.ShortColor;
        public SolidColorBrush PrevSupertrend3Color => PrevSupertrend3 >= 0 ? Common.LongColor : Common.ShortColor;

        public PairQuote(string symbol, IEnumerable<Quote> quotes)
        {
            Symbol = symbol;
            Charts = quotes.Select(quote => new ChartInfo(quote)).ToList();
        }

        public void UpdateQuote(Quote quote)
        {
            try
            {
                var lastQuote = Charts[^1];
                if (lastQuote.Quote.Date.Equals(quote.Date) || quote.Date.Minute % Common.BaseIntervalNumber != 0)
                {
                    lastQuote.Quote.High = quote.High;
                    lastQuote.Quote.Low = quote.Low;
                    lastQuote.Quote.Close = quote.Close;
                    lastQuote.Quote.Volume = quote.Volume;
                }
                else
                {
                    Charts.Add(new ChartInfo(quote));
                }
            }
            catch (Exception ex)
            {
                Logger.Log(nameof(PairQuote), MethodBase.GetCurrentMethod()?.Name, ex);
            }
        }

        public void UpdateIndicators()
        {
            try
            {
                var ts = Charts.Select(x => x.Quote).GetTripleSupertrend(10, 1.2, 10, 3, 10, 10);

                for (int i = 0; i < Charts.Count; i++)
                {
                    Charts[i].Supertrend1 = ts.ElementAt(i).Supertrend1;
                    Charts[i].Supertrend2 = ts.ElementAt(i).Supertrend2;
                    Charts[i].Supertrend3 = ts.ElementAt(i).Supertrend3;
                }
            }
            catch (Exception ex)
            {
                Logger.Log(nameof(PairQuote), MethodBase.GetCurrentMethod()?.Name, ex);
            }
        }
    }
}
