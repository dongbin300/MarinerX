using Albedo.Enums;
using Albedo.Models;

using System;
using System.ComponentModel;
using System.Windows.Media.Imaging;

namespace Albedo.ViewModels
{
    public class PairControlViewModel : INotifyPropertyChanged
    {
        #region Notify Property Changed
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion Notify Property Changed

        private string symbol = string.Empty;
        public string Symbol
        {
            get => symbol;
            set
            {
                symbol = value;
                OnPropertyChanged(nameof(Symbol));
            }
        }
        private PairMarket market = PairMarket.None;
        public PairMarket Market
        {
            get => market;
            set
            {
                market = value;
                OnPropertyChanged(nameof(Market));
            }
        }
        private PairMarketType marketType = PairMarketType.None;
        public PairMarketType MarketType
        {
            get => marketType;
            set
            {
                marketType = value;
                OnPropertyChanged(nameof(MarketType));
            }
        }
        private decimal price = 0;
        public decimal Price
        {
            get => price;
            set
            {
                price = value;
                OnPropertyChanged(nameof(Price));
                OnPropertyChanged(nameof(PriceString));
            }
        }
        private decimal priceChangePercent = 0;
        public decimal PriceChangePercent
        {
            get => priceChangePercent;
            set
            {
                priceChangePercent = value;
                OnPropertyChanged(nameof(PriceChangePercent));
                OnPropertyChanged(nameof(PriceChangePercentString));
                OnPropertyChanged(nameof(IsBullish));
            }
        }
        private BitmapImage marketIcon = new();
        public BitmapImage MarketIcon
        {
            get => marketIcon;
            set
            {
                marketIcon = value;
                OnPropertyChanged(nameof(MarketIcon));
            }
        }
        private bool isSelected = false;
        public bool IsSelected
        {
            get => isSelected;
            set
            {
                isSelected = value;
                OnPropertyChanged(nameof(IsSelected));
            }
        }

        public string PriceString => price.ToString();
        public string PriceChangePercentString => Math.Round(priceChangePercent, 2) + "%";
        public bool IsBullish => priceChangePercent >= 0;

        public PairControlViewModel(Pair pair)
        {
            Symbol = pair.Symbol;
            Market = pair.Market;
            MarketType = pair.MarketType;
            Price = pair.Price;
            PriceChangePercent = pair.PriceChangePercent;

            MarketIcon = new BitmapImage(new Uri("pack://application:,,,/Albedo;component/Resources/" + Market switch
            {
                PairMarket.Binance => "binance.png",
                _ => ""
            }));
        }
    }
}
