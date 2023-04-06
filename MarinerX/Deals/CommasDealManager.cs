using MercuryTradingModel.Charts;

using System.Collections.Generic;
using System.Linq;

namespace MarinerX.Deals
{
    public class CommasDealManager
    {
        public List<CommasDeal> Deals { get; set; } = new List<CommasDeal>();
        public CommasDeal? LatestDeal => Deals.Count > 0 ? Deals[^1] : null;
        public decimal CurrentPositionQuantity => GetCurrentPositionQuantity();
        public decimal TotalIncome => GetIncome();
        public ChartInfo ChartInfo { get; set; } = new("", new Skender.Stock.Indicators.Quote());
        public decimal Upnl => GetUpnl(ChartInfo);
        public decimal EstimatedTotalIncome => TotalIncome + Upnl;
        public decimal TargetRoe { get; set; }

        public void OpenDeal(ChartInfo info)
        {
            Deals.Add(new CommasDeal
            {
                OpenTime = info.DateTime,
                BuyPrice = (info.Quote.High + info.Quote.Low) / 2,
                BuyQuantity = 1
            });
        }

        public void CloseDeal(ChartInfo info)
        {
            if (LatestDeal == null || LatestDeal.IsClosed)
            {
                return;
            }

            LatestDeal.CloseTime = info.DateTime;
            LatestDeal.SellPrice = (info.Quote.High + info.Quote.Low) / 2;
            LatestDeal.SellQuantity = 1;
        }

        public decimal GetUpnl(ChartInfo info)
        {
            var inProgressDeals = Deals.Where(d => !d.IsClosed);
            if (inProgressDeals == null)
            {
                return 0;
            }

            return inProgressDeals.Sum(d => (info.Quote.Close - d.BuyPrice) * d.BuyQuantity);
        }

        public decimal GetCurrentPositionQuantity()
        {
            if (LatestDeal == null || LatestDeal.IsClosed)
            {
                return 0;
            }

            return LatestDeal.BuyQuantity;
        }

        public decimal GetCurrentRoe(ChartInfo info)
        {
            if (LatestDeal == null || LatestDeal.IsClosed)
            {
                return 0;
            }

            return LatestDeal.GetCurrentRoe(info);
        }

        public decimal GetIncome()
        {
            return Deals.Sum(d => d.Income);
        }
    }
}
