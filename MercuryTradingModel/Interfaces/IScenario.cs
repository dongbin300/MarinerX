namespace MercuryTradingModel.Interfaces
{
    public interface IScenario
    {
        public string Name { get; set; }
        public IList<IStrategy> Strategies { get; set; }

        public IScenario AddStrategy(IStrategy strategy);
    }
}
