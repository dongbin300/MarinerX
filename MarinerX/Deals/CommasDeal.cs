using MercuryTradingModel.Charts;
using MercuryTradingModel.Maths;

using System;

namespace MarinerX.Deals
{
    public class CommasDeal
    {
        public DateTime OpenTime { get; set; }
        public DateTime CloseTime { get; set; }
        public bool IsClosed => CloseTime >= new DateTime(2000, 1, 1);
        public TimeSpan TakenTime => CloseTime - OpenTime;
        public decimal BuyPrice { get; set; } = 0;
        public decimal SellPrice { get; set; } = 0;
        public decimal BuyQuantity { get; set; } = 0;
        public decimal SellQuantity { get; set; } = 0;
        public decimal Income => (SellPrice - BuyPrice) * SellQuantity;
        public decimal Roe => StockUtil.Roe(MercuryTradingModel.Enums.PositionSide.Long, BuyPrice, SellPrice);

        public override string ToString()
        {
            return $"{TakenTime}, {Income}, {Roe}%";
        }

        public decimal GetCurrentRoe(ChartInfo info)
        {
            return StockUtil.Roe(MercuryTradingModel.Enums.PositionSide.Long, BuyPrice, (info.Quote.Low + info.Quote.High) / 2);
        }
    }
}
