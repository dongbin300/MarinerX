using MercuryTradingModel.Assets;
using MercuryTradingModel.Charts;

namespace MercuryTradingModel.Interfaces
{
    public interface ISignal
    {
        public IFormula Formula { get; set; }
        public abstract bool IsFlare(Asset asset, ChartInfo chart, ChartInfo prevChart);
    }
}
