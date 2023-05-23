using System.Collections.Generic;

namespace Albedo.Models
{
    public class Indicator
    {
        public int Id { get; set; }
        public List<IndicatorData> Data { get; set; }

        public Indicator(int id, List<IndicatorData> data)
        {
            Id = id;
            Data = data;
        }
    }
}
