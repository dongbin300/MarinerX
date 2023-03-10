using MercuryTradingModel.Assets;
using MercuryTradingModel.Charts;

namespace MercuryTradingModel.Interfaces
{
    public interface ISignal
    {
        IFormula Formula { get; set; }
        abstract bool IsFlare(Asset asset, ChartInfo chart, ChartInfo prevChart);
    }
}
