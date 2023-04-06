using MercuryTradingModel.Charts;
using MercuryTradingModel.Maths;

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
        public decimal MaxSafetyOrderCount { get; set; }
        public decimal Deviation { get; set; }
        public List<decimal> Deviations { get; private set; } = new();

        public CommasDealManager(decimal targetRoe, decimal maxSafetyOrderCount, decimal deviation)
        {
            TargetRoe = targetRoe;
            MaxSafetyOrderCount = maxSafetyOrderCount;
            Deviation = deviation;
            for (int i = 1; i <= maxSafetyOrderCount; i++)
            {
                Deviations.Add(deviation * i);
            }
        }

        /// <summary>
        /// 포지션 진입
        /// </summary>
        /// <param name="info"></param>
        /// <param name="price"></param>
        /// <param name="quantity"></param>
        public void OpenDeal(ChartInfo info, decimal? price = null, decimal? quantity = null)
        {
            var deal = new CommasDeal();
            deal.OpenTransactions.Add(new CommasOpenTransaction
            {
                Time = info.DateTime,
                Price = price == null ? (info.Quote.High + info.Quote.Low) / 2 : price.Value,
                Quantity = quantity == null ? 1 : quantity.Value
            });
            Deals.Add(deal);
        }

        /// <summary>
        /// 추가 포지셔닝
        /// </summary>
        /// <param name="info"></param>
        public void AdditionalDeal(ChartInfo info)
        {
            if (LatestDeal == null || LatestDeal.IsClosed)
            {
                return;
            }

            LatestDeal.OpenTransactions.Add(new CommasOpenTransaction
            {
                Time = info.DateTime,
                Price = StockUtil.GetPriceByRoe(MercuryTradingModel.Enums.PositionSide.Long, LatestDeal.BuyAveragePrice, -Deviations[LatestDeal.CurrentSafetyOrderCount]), // 정확히 추가매수 가격에서 매수
                Quantity = 1
            });
        }

        /// <summary>
        /// 전량 정리
        /// </summary>
        /// <param name="info"></param>
        public void CloseDeal(ChartInfo info)
        {
            if (LatestDeal == null || LatestDeal.IsClosed)
            {
                return;
            }

            LatestDeal.CloseTransaction.Time = info.DateTime;
            LatestDeal.CloseTransaction.Price = StockUtil.GetPriceByRoe(MercuryTradingModel.Enums.PositionSide.Long, LatestDeal.BuyAveragePrice, TargetRoe); // 정확히 목표ROE 가격에서 매도
            LatestDeal.CloseTransaction.Quantity = LatestDeal.BuyQuantity;
        }

        public decimal GetUpnl(ChartInfo info)
        {
            var inProgressDeals = Deals.Where(d => !d.IsClosed);
            if (inProgressDeals == null)
            {
                return 0;
            }

            return inProgressDeals.Sum(d => (info.Quote.Close - d.BuyAveragePrice) * d.BuyQuantity);
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

        public bool IsAdditionalOpen(ChartInfo info)
        {
            if (LatestDeal == null)
            {
                return false;
            }

            if (LatestDeal.CurrentSafetyOrderCount == MaxSafetyOrderCount)
            {
                return false;
            }

            return StockUtil.Roe(MercuryTradingModel.Enums.PositionSide.Long, LatestDeal.BuyAveragePrice, info.Quote.Low) <= -Deviations[LatestDeal.CurrentSafetyOrderCount];
        }
    }
}
