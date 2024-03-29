﻿namespace CryptoModel.Backtests
{
    public class Transaction
    {
        public DateTime Time { get; set; }
        public decimal Price { get; set; }
        public decimal Quantity { get; set; }

        public override string ToString()
        {
            return $"{Time}, {Price}, {Quantity}";
        }
    }
}
