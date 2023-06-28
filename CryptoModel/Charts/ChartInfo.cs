﻿using Skender.Stock.Indicators;

namespace CryptoModel.Charts
{
    public class ChartInfo
    {
        public string Symbol { get; set; }
        public DateTime DateTime => Quote.Date;
        public Quote Quote { get; set; }

        public double Sma1 { get; set; }
        public double Sma2 { get; set; }
        public double Sma3 { get; set; }
        public double Ema1 { get; set; }
        public double Ema2 { get; set; }
        public double Ema3 { get; set; }
        public double Lsma1 { get; set; }
        public double Lsma2 { get; set; }
        public double Lsma3 { get; set; }
        public double Rsi { get; set; }
        public double JmaSlope { get; set; }
        public double K { get; set; }
        public double D { get; set; }
        public double Supertrend1 { get; set; }
        public double Supertrend2 { get; set; }
        public double Supertrend3 { get; set; }

        public ChartInfo(string symbol, Quote quote)
        {
            Symbol = symbol;
            Quote = quote;
        }

        public override string ToString()
        {
            return $"{Symbol}, {DateTime}, {Quote.Open}:{Quote.High}:{Quote.Low}:{Quote.Close}:{Quote.Volume}";
        }
    }
}
