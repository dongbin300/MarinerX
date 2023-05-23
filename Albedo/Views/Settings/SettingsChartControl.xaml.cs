using Albedo.Managers;
using Albedo.Models;

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
            MaPeriodText1.Text = SettingsMan.MaPeriod1.ToString();
            MaTypeCombo1.SelectedItem = SettingsMan.MaType1;
            MaLineColorCombo1.SelectedItem = SettingsMan.MaLineColor1;
            MaLineWeightCombo1.SelectedItem = SettingsMan.MaLineWeight1;
            MaEnable1.IsChecked = Albedo.Settings.Default.MaEnable1;
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

        /// <summary>
        /// 이평선 기간 수정
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MaPeriodText_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (isInit)
            {
                return;
            }

            if (int.TryParse(MaPeriodText1.Text, out var period))
            {
                if (period >= 1 && period <= 1000)
                {
                    Albedo.Settings.Default.MaPeriod1 = period;
                }
                else
                {
                    Albedo.Settings.Default.MaPeriod1 = Common.DefaultMaPeriod;
                }
            }
            else
            {
                Albedo.Settings.Default.MaPeriod1 = Common.DefaultMaPeriod;
            }
            Albedo.Settings.Default.Save();
        }

        /// <summary>
        /// 이평선 타입 수정
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MaTypeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isInit)
            {
                return;
            }

            if (MaTypeCombo1.SelectedItem is not MaTypeModel model)
            {
                return;
            }

            Albedo.Settings.Default.MaType1 = model.Type.ToString();
            Albedo.Settings.Default.Save();
        }

        private void MaLineColorCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isInit)
            {
                return;
            }

            if (MaLineColorCombo1.SelectedItem is not LineColorModel model)
            {
                return;
            }

            Albedo.Settings.Default.MaLineColor1 = model.LineColor.ToString();
            Albedo.Settings.Default.Save();
        }

        private void MaLineWeightCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isInit)
            {
                return;
            }

            if (MaLineWeightCombo1.SelectedItem is not LineWeightModel model)
            {
                return;
            }

            Albedo.Settings.Default.MaLineWeight1 = model.LineWeight.ToString();
            Albedo.Settings.Default.Save();
        }

        private void MaEnable_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (isInit)
            {
                return;
            }

            Albedo.Settings.Default.MaEnable1 = MaEnable1.IsChecked ?? false;
            Albedo.Settings.Default.Save();
        }
    }
}
