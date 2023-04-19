using Albedo.Models;

using System;
using System.ComponentModel;
using System.Windows.Media.Imaging;

namespace Albedo.ViewModels
{
    public class SymbolControlViewModel : INotifyPropertyChanged
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
        private string diff = string.Empty;
        public string Diff
        {
            get => diff;
            set
            {
                diff = value;
                OnPropertyChanged(nameof(Diff));
            }
        }
        public string TextColor => diff.Length >= 1 && diff[0] != '-' ? "3BCF86" : "ED3161";
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

        public SymbolControlViewModel()
        {

        }

        public void Init(Symbol symbol)
        {
            Symbol = symbol.Name;
            Market = symbol.Market;
            Price = symbol.Price.ToString();
            Diff = Math.Round(symbol.Diff, 2) + "%";

            MarketIcon = new BitmapImage(new Uri("pack://application:,,,/Albedo;component/Resources/" + Market.ToLower() switch
            {
                "binance" => "binance.png",
                _ => ""
            }));
        }
    }
}
