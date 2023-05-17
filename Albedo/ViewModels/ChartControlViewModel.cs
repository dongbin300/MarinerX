using Albedo.Commands;
using Albedo.Enums;

using Binance.Net.Enums;

using System.ComponentModel;
using System.Windows.Input;

namespace Albedo.ViewModels
{
    public class ChartControlViewModel : INotifyPropertyChanged
    {
        #region Notify Property Changed
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion Notify Property Changed

        public ICommand? IntervalClick { get; set; }

        public ChartControlViewModel()
        {
            IntervalClick = new DelegateCommand((obj) =>
            {
                if (obj == null)
                {
                    return;
                }

                Settings.Default.Interval = obj.ToString();
                Settings.Default.Save();
                Common.ChartInterval = obj.ToString() switch
                {
                    "1분" => CandleInterval.OneMinute,
                    "3분" => CandleInterval.ThreeMinutes,
                    "5분" => CandleInterval.FiveMinutes,
                    "10분" => CandleInterval.TenMinutes,
                    "15분" => CandleInterval.FifteenMinutes,
                    "30분" => CandleInterval.ThirtyMinutes,
                    "1시간" => CandleInterval.OneHour,
                    "1일" => CandleInterval.OneDay,
                    "1주" => CandleInterval.OneWeek,
                    "1월" => CandleInterval.OneMonth,
                    _ => CandleInterval.OneMinute
                };

                Common.ChartRefresh.Invoke();
            });
        }
    }
}
