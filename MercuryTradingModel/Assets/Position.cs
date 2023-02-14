using MercuryTradingModel.Enums;

namespace MercuryTradingModel.Assets
{
    public class Position
    {
        public PositionSide Side { get; set; } = PositionSide.None;
        public decimal TransactionAmount { get; set; } = 0m;
        public decimal Amount { get; set; } = 0m;
        public decimal AveragePrice => Amount == 0 ? 0 : TransactionAmount / Amount;
        public decimal Value => Side == PositionSide.Short ? -Amount : Amount;

        public void Long(decimal quantity, decimal price)
        {
            if (Side == PositionSide.Long)
            {
                Amount += quantity;
                TransactionAmount += quantity * price;
            }
            else
            {
                Amount -= quantity;
                TransactionAmount -= quantity * price;
                if (Amount < 0)
                {
                    Side = PositionSide.Long;
                    Amount = -Amount;
                    TransactionAmount = -TransactionAmount;
                }
            }
        }

        public void Short(decimal quantity, decimal price)
        {
            if (Side == PositionSide.Short)
            {
                Amount += quantity;
                TransactionAmount += quantity * price;
            }
            else
            {
                Amount -= quantity;
                TransactionAmount -= quantity * price;
                if (Amount < 0)
                {
                    Side = PositionSide.Short;
                    Amount = -Amount;
                    TransactionAmount = -TransactionAmount;
                }
            }
        }

        public override string ToString()
        {
            return (Side == PositionSide.Long ? "+" : "-") + Amount;
        }
    }
}
