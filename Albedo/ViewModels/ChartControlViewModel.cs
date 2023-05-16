using Albedo.Commands;
using Albedo.Utils;

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
                    "1분" => KlineInterval.OneMinute,
                    "3분" => KlineInterval.ThreeMinutes,
                    "5분" => KlineInterval.FiveMinutes,
                    "30분" => KlineInterval.ThirtyMinutes,
                    "1시간" => KlineInterval.OneHour,
                    "1일" => KlineInterval.OneDay,
                    "1주" => KlineInterval.OneWeek,
                    "1월" => KlineInterval.OneMonth,
                    _ => KlineInterval.OneMinute
                };

                Common.ChartRefresh.Invoke();
            });
        }
    }
}
