﻿namespace MercuryTradingModel.Interfaces
{
    public interface IStrategy
    {
        public string Name { get; set; }
        public ICue? Cue { get; set; }
        public ISignal Signal { get; set; }
        public IOrder Order { get; set; }
        public string Tag { get; set; }
    }
}
