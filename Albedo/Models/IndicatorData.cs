using System;

namespace Albedo.Models
{
    public class IndicatorData
    {
        public DateTime Date { get; set; }
        public decimal Value { get; set; }

        public IndicatorData(DateTime date, decimal value)
        {
            Date = date;
            Value = value;
        }
    }
}
