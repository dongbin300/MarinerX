using Albedo.Commands;
using Albedo.Enums;
using Albedo.Models;
using Albedo.Utils;
using Albedo.Views;

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;

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

        private ObservableCollection<PairControl> pairControls = new();
        public ObservableCollection<PairControl> PairControls
        {
            get => pairControls;
            set
            {
                pairControls = value;
                OnPropertyChanged(nameof(PairControls));
            }
        }
        private ObservableCollection<PairControl> resultPairControls = new();
        public ObservableCollection<PairControl> ResultPairControls
        {
            get => resultPairControls;
            set
            {
                resultPairControls = value;
                OnPropertyChanged(nameof(ResultPairControls));
            }
        }
        private ObservableCollection<PairMarketModel> pairMarkets = new();
        public ObservableCollection<PairMarketModel> PairMarkets
        {
            get => pairMarkets;
            set
            {
                pairMarkets = value;
                OnPropertyChanged(nameof(PairMarkets));
            }
        }
        private ObservableCollection<PairMarketTypeModel> pairMarketTypes = new();
        public ObservableCollection<PairMarketTypeModel> PairMarketTypes
        {
            get => pairMarketTypes;
            set
            {
                pairMarketTypes = value;
                OnPropertyChanged(nameof(PairMarketTypes));
            }
        }
        private ObservableCollection<PairQuoteAssetModel> pairQuoteAssets = new();
        public ObservableCollection<PairQuoteAssetModel> PairQuoteAssets
        {
            get => pairQuoteAssets;
            set
            {
                pairQuoteAssets = value;
                OnPropertyChanged(nameof(PairQuoteAssets));
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
        private int selectedPairQuoteAssetIndex = 0;
        public int SelectedPairQuoteAssetIndex
        {
            get => selectedPairQuoteAssetIndex;
            set
            {
                selectedPairQuoteAssetIndex = value;
                OnPropertyChanged(nameof(SelectedPairQuoteAssetIndex));
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

        public ICommand? PairMarketSelectionChanged { get; set; }
        public ICommand? PairMarketTypeSelectionChanged { get; set; }
        public ICommand? PairQuoteAssetSelectionChanged { get; set; }
        public ICommand? PairSelectionChanged { get; set; }

        public MenuControlViewModel()
        {
            InitEvent();
            InitMarket();
            InitMarketType();
            InitQuoteAsset();
        }

        /// <summary>
        /// 이벤트 초기화
        /// </summary>
        private void InitEvent()
        {
            // 거래소 변경 이벤트
            PairMarketSelectionChanged = new DelegateCommand((obj) =>
            {
                if (obj is not PairMarketModel market)
                {
                    return;
                }

                Common.CurrentSelectedPairMarket = market;
                PairControls.Clear();
                InitMarketType();
            });

            // 타입 변경 이벤트
            PairMarketTypeSelectionChanged = new DelegateCommand((obj) =>
            {
                if (obj is not PairMarketTypeModel marketType)
                {
                    return;
                }

                Common.CurrentSelectedPairMarketType = marketType;
                PairControls.Clear();
                InitQuoteAsset();
            });

            // 거래자산 변경 이벤트
            PairQuoteAssetSelectionChanged = new DelegateCommand((obj) =>
            {
                if (obj is not PairQuoteAssetModel quoteAsset)
                {
                    return;
                }

                Common.CurrentSelectedPairQuoteAsset = quoteAsset;
                PairControls.Clear();
            });

            // 코인 선택 변경 이벤트
            PairSelectionChanged = new DelegateCommand((obj) =>
            {
                if (obj is not PairControl pairControl)
                {
                    return;
                }

                Common.Pair = pairControl.Pair;
                Common.ChartRefresh();
            });
        }

        /// <summary>
        /// 거래소 초기화
        /// </summary>
        private void InitMarket()
        {
            PairMarkets.Clear();
            if (Common.SupportedMarket.HasFlag(PairMarket.Binance))
            {
                PairMarkets.Add(new PairMarketModel(PairMarket.Binance, "바이낸스", "Resources/binance.png"));
            }
            if (Common.SupportedMarket.HasFlag(PairMarket.Upbit))
            {
                PairMarkets.Add(new PairMarketModel(PairMarket.Upbit, "업비트", "Resources/upbit.png"));
            }
            if (Common.SupportedMarket.HasFlag(PairMarket.Bithumb))
            {
                PairMarkets.Add(new PairMarketModel(PairMarket.Bithumb, "빗썸", "Resources/bithumb.png"));
            }
            if (PairMarkets.Count > 0)
            {
                SelectedPairMarketIndex = 0;
                PairMarketSelectionChanged?.Execute(PairMarkets[SelectedPairMarketIndex]);
            }
            else
            {
                SelectedPairMarketIndex = -1;
            }
        }

        /// <summary>
        /// 거래소 타입 초기화
        /// </summary>
        private void InitMarketType()
        {
            PairMarketTypes.Clear();
            switch (Common.CurrentSelectedPairMarket.PairMarket)
            {
                case PairMarket.Binance:
                    PairMarketTypes.Add(new PairMarketTypeModel(PairMarketType.Spot, "현물"));
                    PairMarketTypes.Add(new PairMarketTypeModel(PairMarketType.Futures, "선물"));
                    PairMarketTypes.Add(new PairMarketTypeModel(PairMarketType.CoinFutures, "코인 선물"));
                    break;

                case PairMarket.Upbit:
                case PairMarket.Bithumb:
                    PairMarketTypes.Add(new PairMarketTypeModel(PairMarketType.Spot, "현물"));
                    break;
            }
            if (PairMarketTypes.Count > 0)
            {
                SelectedPairMarketTypeIndex = 0;
                PairMarketTypeSelectionChanged?.Execute(PairMarketTypes[SelectedPairMarketTypeIndex]);
            }
            else
            {
                SelectedPairMarketTypeIndex = -1;
            }
        }

        /// <summary>
        /// 거래자산 초기화
        /// </summary>
        private void InitQuoteAsset()
        {
            PairQuoteAssets.Clear();
            switch (Common.CurrentSelectedPairMarket.PairMarket)
            {
                case PairMarket.Binance:
                    switch (Common.CurrentSelectedPairMarketType.PairMarketType)
                    {
                        case PairMarketType.Spot:
                            PairQuoteAssets.Add(new PairQuoteAssetModel(PairQuoteAsset.USDT, "USDT"));
                            PairQuoteAssets.Add(new PairQuoteAssetModel(PairQuoteAsset.TUSD, "TUSD"));
                            PairQuoteAssets.Add(new PairQuoteAssetModel(PairQuoteAsset.BUSD, "BUSD"));
                            PairQuoteAssets.Add(new PairQuoteAssetModel(PairQuoteAsset.BNB, "BNB"));
                            PairQuoteAssets.Add(new PairQuoteAssetModel(PairQuoteAsset.BTC, "BTC"));
                            PairQuoteAssets.Add(new PairQuoteAssetModel(PairQuoteAsset.ETH, "ETH"));
                            PairQuoteAssets.Add(new PairQuoteAssetModel(PairQuoteAsset.DAI, "DAI"));
                            PairQuoteAssets.Add(new PairQuoteAssetModel(PairQuoteAsset.USDC, "USDC"));
                            PairQuoteAssets.Add(new PairQuoteAssetModel(PairQuoteAsset.VAI, "VAI"));
                            PairQuoteAssets.Add(new PairQuoteAssetModel(PairQuoteAsset.XRP, "XRP"));
                            PairQuoteAssets.Add(new PairQuoteAssetModel(PairQuoteAsset.TRX, "TRX"));
                            PairQuoteAssets.Add(new PairQuoteAssetModel(PairQuoteAsset.DOGE, "DOGE"));
                            PairQuoteAssets.Add(new PairQuoteAssetModel(PairQuoteAsset.DOT, "DOT"));
                            PairQuoteAssets.Add(new PairQuoteAssetModel(PairQuoteAsset.AUD, "AUD"));
                            PairQuoteAssets.Add(new PairQuoteAssetModel(PairQuoteAsset.BIDR, "BIDR"));
                            PairQuoteAssets.Add(new PairQuoteAssetModel(PairQuoteAsset.BRL, "BRL"));
                            PairQuoteAssets.Add(new PairQuoteAssetModel(PairQuoteAsset.EUR, "EUR"));
                            PairQuoteAssets.Add(new PairQuoteAssetModel(PairQuoteAsset.GBP, "GBP"));
                            PairQuoteAssets.Add(new PairQuoteAssetModel(PairQuoteAsset.RUB, "RUB"));
                            PairQuoteAssets.Add(new PairQuoteAssetModel(PairQuoteAsset.TRY, "TRY"));
                            PairQuoteAssets.Add(new PairQuoteAssetModel(PairQuoteAsset.UAH, "UAH"));
                            PairQuoteAssets.Add(new PairQuoteAssetModel(PairQuoteAsset.ZAR, "ZAR"));
                            PairQuoteAssets.Add(new PairQuoteAssetModel(PairQuoteAsset.IDRT, "IDRT"));
                            PairQuoteAssets.Add(new PairQuoteAssetModel(PairQuoteAsset.NGN, "NGN"));
                            PairQuoteAssets.Add(new PairQuoteAssetModel(PairQuoteAsset.PLN, "PLN"));
                            PairQuoteAssets.Add(new PairQuoteAssetModel(PairQuoteAsset.RON, "RON"));
                            PairQuoteAssets.Add(new PairQuoteAssetModel(PairQuoteAsset.ARS, "ARS"));
                            PairQuoteAssets.Add(new PairQuoteAssetModel(PairQuoteAsset.USD, "USD"));
                            break;

                        case PairMarketType.Futures:
                            PairQuoteAssets.Add(new PairQuoteAssetModel(PairQuoteAsset.USDT, "USDT"));
                            PairQuoteAssets.Add(new PairQuoteAssetModel(PairQuoteAsset.BUSD, "BUSD"));
                            PairQuoteAssets.Add(new PairQuoteAssetModel(PairQuoteAsset.BTC, "BTC"));
                            break;

                        case PairMarketType.CoinFutures:
                            break;
                    }
                    break;

                case PairMarket.Upbit:
                    PairQuoteAssets.Add(new PairQuoteAssetModel(PairQuoteAsset.KRW, "KRW"));
                    PairQuoteAssets.Add(new PairQuoteAssetModel(PairQuoteAsset.BTC, "BTC"));
                    PairQuoteAssets.Add(new PairQuoteAssetModel(PairQuoteAsset.USDT, "USDT"));
                    break;

                case PairMarket.Bithumb:
                    PairQuoteAssets.Add(new PairQuoteAssetModel(PairQuoteAsset.KRW, "KRW"));
                    PairQuoteAssets.Add(new PairQuoteAssetModel(PairQuoteAsset.BTC, "BTC"));
                    break;
            }
            if (PairQuoteAssets.Count > 0)
            {
                SelectedPairQuoteAssetIndex = 0;
                PairQuoteAssetSelectionChanged?.Execute(pairQuoteAssets[SelectedPairQuoteAssetIndex]);
            }
            else
            {
                SelectedPairQuoteAssetIndex = -1;
            }
        }

        /// <summary>
        /// 코인 검색
        /// </summary>
        public void SearchPair()
        {
            if (string.IsNullOrEmpty(keywordText))
            {
                ResultPairControls = new ObservableCollection<PairControl>(PairControls);
                return;
            }

            ResultPairControls = new ObservableCollection<PairControl>(PairControls.Where(p => p.Pair.Symbol.Contains(keywordText)));
        }

        /// <summary>
        /// 코인 정보(이름, 가격, 등락률) 업데이트
        /// </summary>
        /// <param name="pair"></param>
        public void UpdatePairInfo(Pair pair)
        {
            var pairTag = $"{pair.Market}_{pair.MarketType}_{pair.Symbol}";
            var _pair = PairControls.Where(p => p.Tag.Equals(pairTag));

            if (_pair == null || !_pair.Any())
            {
                var pairControl = new PairControl();
                pairControl.Init(pair);
                PairControls.Add(pairControl);
                return;
            }

            _pair.ElementAt(0).Pair.Price = pair.Price;
            _pair.ElementAt(0).Pair.PriceChangePercent = pair.PriceChangePercent;
        }
    }
}
