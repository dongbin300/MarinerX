using MercuryTradingModel.Charts;
using MercuryTradingModel.Maths;

using System;
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
        public decimal BaseOrderSize { get; set; }
        public decimal SafetyOrderSize { get; set; }
        public int MaxSafetyOrderCount { get; set; }
        public decimal Deviation { get; set; }
        public List<decimal> Deviations { get; private set; } = new();
        public decimal SafetyOrderStepScale { get; set; }
        public decimal SafetyOrderVolumeScale { get; set; }
        public List<decimal> SafetyOrderVolumes { get; private set; } = new();

        public CommasDealManager(decimal targetRoe, decimal baseOrderSize, decimal safetyOrderSize, int maxSafetyOrderCount, decimal deviation, decimal stepScale, decimal volumeScale)
        {
            TargetRoe = targetRoe;
            BaseOrderSize = baseOrderSize;
            SafetyOrderSize = safetyOrderSize;
            MaxSafetyOrderCount = maxSafetyOrderCount;
            Deviation = deviation;
            SafetyOrderStepScale = stepScale;
            SafetyOrderVolumeScale = volumeScale;
            for (int i = 0; i < maxSafetyOrderCount; i++)
            {
                if (i == 0)
                {
                    Deviations.Add(Deviation);
                    SafetyOrderVolumes.Add(SafetyOrderSize);
                }
                else
                {
                    Deviations.Add(Deviations[i - 1] + Deviation * (decimal)Math.Pow((double)SafetyOrderStepScale, i));
                    SafetyOrderVolumes.Add(SafetyOrderVolumes[i - 1] + SafetyOrderSize * (decimal)Math.Pow((double)SafetyOrderVolumeScale, i));
                }
            }
        }

        /// <summary>
        /// 매매 확인
        /// </summary>
        /// <param name="info"></param>
        public void Evaluate(ChartInfo info)
        {
            var roe = GetCurrentRoe(info);
            var rsi = info.GetChartElementValue(MercuryTradingModel.Enums.ChartElementType.rsi);

            // 포지션이 없고 RSI<30 이면 매수
            if (CurrentPositionQuantity < 0.0001m && rsi > 0 && rsi < 30)
            {
                var price = (info.Quote.High + info.Quote.Low) / 2;
                var quantity = BaseOrderSize / price;
                OpenDeal(info, price, quantity);
            }
            // 포지션이 있고 추가 매수 지점에 도달하면 추가 매수
            else if (CurrentPositionQuantity > 0.0001m && IsAdditionalOpen(info))
            {
                var price = StockUtil.GetPriceByRoe(MercuryTradingModel.Enums.PositionSide.Long, LatestDeal.BuyAveragePrice, -Deviations[LatestDeal.CurrentSafetyOrderCount]);
                var quantity = SafetyOrderVolumes[LatestDeal.CurrentSafetyOrderCount] / price;
                AdditionalDeal(info, price, quantity);
            }
            // 포지션이 있고 목표 수익률에 도달하면 매도
            else if (CurrentPositionQuantity > 0.0001m && roe >= TargetRoe)
            {
                CloseDeal(info);
            }
        }

        /// <summary>
        /// 포지션 진입
        /// </summary>
        /// <param name="info"></param>
        /// <param name="price"></param>
        /// <param name="quantity"></param>
        public void OpenDeal(ChartInfo info, decimal price, decimal quantity)
        {
            var deal = new CommasDeal();
            deal.OpenTransactions.Add(new CommasOpenTransaction
            {
                Time = info.DateTime,
                Price = price,
                Quantity = quantity
            });
            Deals.Add(deal);
        }

        /// <summary>
        /// 추가 포지셔닝
        /// </summary>
        /// <param name="info"></param>
        public void AdditionalDeal(ChartInfo info, decimal price, decimal quantity)
        {
            if (LatestDeal == null || LatestDeal.IsClosed)
            {
                return;
            }

            LatestDeal.OpenTransactions.Add(new CommasOpenTransaction
            {
                Time = info.DateTime,
                Price = price,
                Quantity = quantity
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
