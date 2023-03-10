using MercuryTradingModel.Assets;
using MercuryTradingModel.Charts;

namespace MercuryTradingModel.Interfaces
{
    public interface ICue
    {
        IFormula Formula { get; set; }
        int Life { get; set; }
        int CurrentLife { get; set; }
        abstract bool CheckFlare(Asset asset, ChartInfo chart, ChartInfo prevChart);
        abstract void Expire();
    }
}
