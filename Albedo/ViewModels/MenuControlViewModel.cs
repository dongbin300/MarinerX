using Albedo.Enums;
using Albedo.Models;
using Albedo.Utils;

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace Albedo.ViewModels
{
    public class MenuControlViewModel : INotifyPropertyChanged
    {
        #region Notify Property Changed
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion Notify Property Changed

        private ObservableCollection<Pair> pairs = new ();
        public ObservableCollection<Pair> Pairs
        {
            get => pairs;
            set
            {
                pairs = value;
                OnPropertyChanged(nameof(Pairs));
            }
        }
        private ObservableCollection<PairMarket> pairMarkets = new();
        public ObservableCollection<PairMarket> PairMarkets
        {
            get => pairMarkets;
            set
            {
                pairMarkets = value;
                OnPropertyChanged(nameof(PairMarkets));
            }
        }
        private ObservableCollection<PairMarketType> pairMarketTypes = new();
        public ObservableCollection<PairMarketType> PairMarketTypes
        {
            get => pairMarketTypes;
            set
            {
                pairMarketTypes = value;
                OnPropertyChanged(nameof(PairMarketTypes));
            }
        }
        private int selectedPairMarketIndex = 0;
        public int SelectedPairMarketIndex
        {
            get => selectedPairMarketIndex;
            set
            {
                selectedPairMarketIndex = value;
                OnPropertyChanged(nameof(SelectedPairMarketIndex));
            }
        }
        private int selectedPairMarketTypeIndex = 0;
        public int SelectedPairMarketTypeIndex
        {
            get => selectedPairMarketTypeIndex;
            set
            {
                selectedPairMarketTypeIndex = value;
                OnPropertyChanged(nameof(SelectedPairMarketTypeIndex));
            }
        }
        private string keywordText = string.Empty;
        public string KeywordText
        {
            get => keywordText;
            set
            {
                keywordText = value;
                OnPropertyChanged(nameof(KeywordText));
                Common.SearchKeywordChanged();
            }
        }

        public MenuControlViewModel()
        {
            PairMarkets.Add(PairMarket.Binance);
            PairMarketTypes.Add(PairMarketType.Spot);
            PairMarketTypes.Add(PairMarketType.Futures);
            PairMarketTypes.Add(PairMarketType.CoinFutures);
            SelectedPairMarketIndex = 0;
            SelectedPairMarketTypeIndex = 0;
        }

        public void UpdatePairInfo(Pair pair)
        {
            var _pair = Pairs.Where(s => s.Market.Equals(pair.Market) && s.Symbol.Equals(pair.Symbol));
            if (_pair == null || !_pair.Any())
            {
                Pairs.Add(pair);
                return;
            }

            _pair.ElementAt(0).Price = pair.Price;
            _pair.ElementAt(0).PriceChangePercent = pair.PriceChangePercent;
        }
    }
}
