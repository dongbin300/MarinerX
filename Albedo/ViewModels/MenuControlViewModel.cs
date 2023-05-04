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

        public ICommand? PairMarketSelectionChanged { get; set; }
        public ICommand? PairMarketTypeSelectionChanged { get; set; }
        public ICommand? PairSelectionChanged { get; set; }

        public MenuControlViewModel()
        {
            // 거래소 및 타입 초기화
            PairMarkets.Add(new PairMarketModel(PairMarket.Binance, "바이낸스", "Resources/binance.png"));
            PairMarkets.Add(new PairMarketModel(PairMarket.Upbit, "업비트", "Resources/upbit.png"));
            PairMarkets.Add(new PairMarketModel(PairMarket.Bithumb, "빗썸", "Resources/bithumb.png"));
            PairMarketTypes.Add(new PairMarketTypeModel(PairMarketType.Spot, "현물"));
            PairMarketTypes.Add(new PairMarketTypeModel(PairMarketType.SpotUsdt, "현물(USDT)"));
            PairMarketTypes.Add(new PairMarketTypeModel(PairMarketType.SpotTusd, "현물(TUSD)"));
            PairMarketTypes.Add(new PairMarketTypeModel(PairMarketType.SpotBusd, "현물(BUSD)"));
            PairMarketTypes.Add(new PairMarketTypeModel(PairMarketType.SpotBnb, "현물(BNB)"));
            PairMarketTypes.Add(new PairMarketTypeModel(PairMarketType.SpotBtc, "현물(BTC)"));
            PairMarketTypes.Add(new PairMarketTypeModel(PairMarketType.SpotEth, "현물(ETH)"));
            PairMarketTypes.Add(new PairMarketTypeModel(PairMarketType.SpotDai, "현물(DAI)"));
            PairMarketTypes.Add(new PairMarketTypeModel(PairMarketType.SpotUsdc, "현물(USDC)"));
            PairMarketTypes.Add(new PairMarketTypeModel(PairMarketType.SpotVai, "현물(VAI)"));
            PairMarketTypes.Add(new PairMarketTypeModel(PairMarketType.SpotXrp, "현물(XRP)"));
            PairMarketTypes.Add(new PairMarketTypeModel(PairMarketType.SpotTrx, "현물(TRX)"));
            PairMarketTypes.Add(new PairMarketTypeModel(PairMarketType.SpotDoge, "현물(DOGE)"));
            PairMarketTypes.Add(new PairMarketTypeModel(PairMarketType.SpotDot, "현물(DOT)"));
            PairMarketTypes.Add(new PairMarketTypeModel(PairMarketType.SpotAud, "현물(AUD)"));
            PairMarketTypes.Add(new PairMarketTypeModel(PairMarketType.SpotBidr, "현물(BIDR)"));
            PairMarketTypes.Add(new PairMarketTypeModel(PairMarketType.SpotBrl, "현물(BRL)"));
            PairMarketTypes.Add(new PairMarketTypeModel(PairMarketType.SpotEur, "현물(EUR)"));
            PairMarketTypes.Add(new PairMarketTypeModel(PairMarketType.SpotGbp, "현물(GBP)"));
            PairMarketTypes.Add(new PairMarketTypeModel(PairMarketType.SpotRub, "현물(RUB)"));
            PairMarketTypes.Add(new PairMarketTypeModel(PairMarketType.SpotTry, "현물(TRY)"));
            PairMarketTypes.Add(new PairMarketTypeModel(PairMarketType.SpotUah, "현물(UAH)"));
            PairMarketTypes.Add(new PairMarketTypeModel(PairMarketType.SpotZar, "현물(ZAR)"));
            PairMarketTypes.Add(new PairMarketTypeModel(PairMarketType.SpotIdrt, "현물(IDRT)"));
            PairMarketTypes.Add(new PairMarketTypeModel(PairMarketType.SpotNgn, "현물(NGN)"));
            PairMarketTypes.Add(new PairMarketTypeModel(PairMarketType.SpotPln, "현물(PLN)"));
            PairMarketTypes.Add(new PairMarketTypeModel(PairMarketType.SpotRon, "현물(RON)"));
            PairMarketTypes.Add(new PairMarketTypeModel(PairMarketType.SpotArs, "현물(ARS)"));
            PairMarketTypes.Add(new PairMarketTypeModel(PairMarketType.Futures, "선물"));
            PairMarketTypes.Add(new PairMarketTypeModel(PairMarketType.CoinFutures, "코인 선물"));
            SelectedPairMarketIndex = 0;
            SelectedPairMarketTypeIndex = 1;

            // 거래소 변경 이벤트
            PairMarketSelectionChanged = new DelegateCommand((obj) =>
            {
                if (obj is not PairMarketModel market)
                {
                    return;
                }

                Common.CurrentSelectedPairMarket = market;
                PairControls.Clear();
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

            // 최초 호출
            PairMarketSelectionChanged?.Execute(pairMarkets[0]);
            PairMarketTypeSelectionChanged?.Execute(PairMarketTypes[1]);
        }

        /// <summary>
        /// 코인 검색
        /// </summary>
        public void SearchPair()
        {
            if (string.IsNullOrEmpty(keywordText))
            {
                ResultPairControls = PairControls;
                return;
            }

            var pairs = PairControls.Where(p => p.Pair.Symbol.Contains(keywordText));
            ResultPairControls = new ObservableCollection<PairControl>(pairs);
        }

        /// <summary>
        /// 코인 정보(이름, 가격, 등락률) 업데이트
        /// </summary>
        /// <param name="pair"></param>
        public void UpdatePairInfo(Pair pair)
        {
            var _pair = PairControls.Where(p => p.Pair.Market.Equals(pair.Market) && p.Pair.Symbol.Equals(pair.Symbol));
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
