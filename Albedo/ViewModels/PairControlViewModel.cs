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
        private string market = string.Empty;
        public string Market
        {
            get => market;
            set
            {
                market = value;
                OnPropertyChanged(nameof(Market));
            }
        }
        private string price = string.Empty;
        public string Price
        {
            get => price;
            set
            {
                price = value;
                OnPropertyChanged(nameof(Price));
            }
        }
        private string priceChangePercent = string.Empty;
        public string PriceChangePercent
        {
            get => priceChangePercent;
            set
            {
                priceChangePercent = value;
                OnPropertyChanged(nameof(PriceChangePercent));
            }
        }
        public string TextColor => priceChangePercent.Length >= 1 && priceChangePercent[0] != '-' ? "3BCF86" : "ED3161";
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

        public PairControlViewModel()
        {

        }

        public void Init(Pair symbol)
        {
            Symbol = symbol.Symbol;
            Market = symbol.Market;
            Price = symbol.Price.ToString();
            PriceChangePercent = Math.Round(symbol.PriceChangePercent, 2) + "%";

            MarketIcon = new BitmapImage(new Uri("pack://application:,,,/Albedo;component/Resources/" + Market.ToLower() switch
            {
                "binance" => "binance.png",
                _ => ""
            }));
        }
    }
}
