using Albedo.Managers;
using Albedo.Models;

using System.Linq;
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

            MaTypeCombo1.Items.Clear();
            foreach (var item in Common.MaTypes)
            {
                MaTypeCombo1.Items.Add(item);
            }

            MaLineColorCombo1.Items.Clear();
            foreach (var item in Common.MaLineColors)
            {
                MaLineColorCombo1.Items.Add(item);
            }

            MaLineWeightCombo1.Items.Clear();
            foreach (var item in Common.MaLineWeights)
            {
                MaLineWeightCombo1.Items.Add(item);
            }

            DefaultCandleCountErrorText.Visibility = Visibility.Hidden;
            isInit = true;
            LoadSettings();
            isInit = false;
        }

        private void LoadSettings()
        {
            DefaultCandleCountText.Text = SettingsMan.DefaultCandleCount.ToString();

            // 이평선 1
            MaEnable1.IsChecked = SettingsMan.Indicators.Mas[0].Enable;
            MaPeriodText1.Text = SettingsMan.Indicators.Mas[0].Period.ToString();
            MaTypeCombo1.SelectedItem = MaTypeCombo1.Items.OfType<MaTypeModel>().First(x=>x.Type.Equals(SettingsMan.Indicators.Mas[0].Type.Type));
            MaLineColorCombo1.SelectedItem = MaLineColorCombo1.Items.OfType<LineColorModel>().First(x => x.LineColor.Equals(SettingsMan.Indicators.Mas[0].LineColor.LineColor));
            MaLineWeightCombo1.SelectedItem = MaLineWeightCombo1.Items.OfType<LineWeightModel>().First(x => x.LineWeight.Equals(SettingsMan.Indicators.Mas[0].LineWeight.LineWeight));
        }

        /// <summary>
        /// 기본 캔들 개수 수정
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DefaultCandleCountText_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (isInit)
            {
                return;
            }

            if (int.TryParse(DefaultCandleCountText.Text, out var defaultCandleCount))
            {
                if (defaultCandleCount >= 10 && defaultCandleCount <= 1000)
                {
                    DefaultCandleCountErrorText.Visibility = Visibility.Hidden;
                    Albedo.Settings.Default.DefaultCandleCount = defaultCandleCount;
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
