using MercuryTradingModel.Assets;

namespace MercuryTradingModel.Interfaces
{
    public interface IAsset
    {
        /// <summary>
        /// 현금 잔고
        /// </summary>
        public decimal Balance { get; set; }

        /// <summary>
        /// 포지션
        /// </summary>
        public Position Position { get; set; }
    }
}
