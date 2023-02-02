using MercuryTradingModel.Assets;
using MercuryTradingModel.Charts;
using MercuryTradingModel.Enums;
using MercuryTradingModel.Trades;

namespace MercuryTradingModel.Interfaces
{
    public interface IOrder
    {
        public OrderType Type { get; set; }
        public PositionSide Side { get; set; }
        public OrderAmount Amount { get; set; }
        public decimal? Price { get; set; }
        public decimal MakerFee { get; }
        public decimal TakerFee { get; }

        public BackTestTradeInfo Run(Asset asset, ChartInfo chartm, string tag);
    }
}
