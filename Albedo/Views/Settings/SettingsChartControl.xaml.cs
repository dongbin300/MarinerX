using System.Windows;
using System.Windows.Controls;

namespace Albedo.Views.Settings
{
    /// <summary>
    /// SettingsChartControl.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class SettingsChartControl : UserControl
    {
        bool isInit = false;

        public SettingsChartControl()
        {
            InitializeComponent();

            DefaultCandleCountErrorText.Visibility = Visibility.Hidden;
            isInit = true;
            DefaultCandleCountText.Text = Albedo.Settings.Default.DefaultCandleCount.ToString();
            isInit = false;
        }

        private void DefaultCandleCountText_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (isInit)
            {
                return;
            }

            if (int.TryParse(DefaultCandleCountText.Text, out var defaulyCandleCount))
            {
                if (defaulyCandleCount >= 10 && defaulyCandleCount <= 1000)
                {
                    DefaultCandleCountErrorText.Visibility = Visibility.Hidden;
                    Albedo.Settings.Default.DefaultCandleCount = defaulyCandleCount;
                }
                else
                {
                    DefaultCandleCountErrorText.Visibility = Visibility.Visible;
                    Albedo.Settings.Default.DefaultCandleCount = Common.ChartDefaultViewCount;
                }
            }
            else
            {
                DefaultCandleCountErrorText.Visibility = Visibility.Visible;
                Albedo.Settings.Default.DefaultCandleCount = Common.ChartDefaultViewCount;
            }
            Albedo.Settings.Default.Save();
        }
    }
}
