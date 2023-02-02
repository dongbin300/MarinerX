using MercuryTradingModel.Assets;
using MercuryTradingModel.Charts;

namespace MercuryTradingModel.Interfaces
{
    public interface ICue
    {
        public IFormula Formula { get; set; }
        public int Life { get; set; }
        public int CurrentLife { get; set; }
        public abstract bool CheckFlare(Asset asset, ChartInfo chart, ChartInfo prevChart);
        public abstract void Expire();
    }
}
