using Albedo.Managers;
using Albedo.Models;
using Albedo.Utils;
using Albedo.Views.Settings;

using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Albedo.Views
{
    /// <summary>
    /// SettingsView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class SettingsView : Window
    {
        private SolidColorBrush selectedColor = new SolidColorBrush(Color.FromRgb(0x49, 0x49, 0x4D));
        private SolidColorBrush transparentColor = new SolidColorBrush(Colors.Transparent);

        private string currentMenu = string.Empty;

        private SettingsChartControl chartControl = new SettingsChartControl();
        private SettingsMarketControl marketControl = new SettingsMarketControl();
        private SettingsPairControl pairControl = new SettingsPairControl();

        public SettingsView()
        {
            InitializeComponent();

            SetDefault();
            currentMenu = "P1";
            R1.Visibility = Visibility.Visible;
            P1.Background = selectedColor;
            T1.FontWeight = FontWeights.Bold;
            MainContent.Content = marketControl;
        }

        private void SetDefault()
        {
            R1.Visibility = Visibility.Hidden;
            P1.Background = transparentColor;
            T1.FontWeight = FontWeights.Regular;
            R2.Visibility = Visibility.Hidden;
            P2.Background = transparentColor;
            T2.FontWeight = FontWeights.Regular;
            R3.Visibility = Visibility.Hidden;
            P3.Background = transparentColor;
            T3.FontWeight = FontWeights.Regular;
        }

        private void SelectedMenuChanged(object sender, MouseButtonEventArgs e)
        {
            if (sender is not StackPanel panel)
            {
                return;
            }

            var menu = panel.Name.ToString();
            if (menu == currentMenu)
            {
                return;
            }

            SetDefault();
            switch (menu)
            {
                case "P1":
                    currentMenu = "P1";
                    R1.Visibility = Visibility.Visible;
                    P1.Background = selectedColor;
                    T1.FontWeight = FontWeights.Bold;
                    MainContent.Content = marketControl;
                    break;

                case "P2":
                    currentMenu = "P2";
                    R2.Visibility = Visibility.Visible;
                    P2.Background = selectedColor;
                    T2.FontWeight = FontWeights.Bold;
                    MainContent.Content = pairControl;
                    break;

                case "P3":
                    currentMenu = "P3";
                    R3.Visibility = Visibility.Visible;
                    P3.Background = selectedColor;
                    T3.FontWeight = FontWeights.Bold;
                    MainContent.Content = chartControl;
                    break;
            }
        }

        /// <summary>
        /// 설정 창을 닫을 때 설정 저장 및 지표 재계산
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closed(object sender, System.EventArgs e)
        {
            try
            {
                // Chart
                SettingsMan.DefaultCandleCount = int.Parse(chartControl.DefaultCandleCountText.Text);
                SettingsMan.Indicators.Mas.Clear();
                SettingsMan.Indicators.Mas.Add(new MaModel(
                    chartControl.MaEnable1.IsChecked ?? false,
                    int.Parse(chartControl.MaPeriodText1.Text),
                    (MaTypeModel)chartControl.MaTypeCombo1.SelectedItem,
                    (LineColorModel)chartControl.MaLineColorCombo1.SelectedItem,
                    (LineWeightModel)chartControl.MaLineWeightCombo1.SelectedItem));
                SettingsMan.Indicators.Mas.Add(new MaModel(
                   chartControl.MaEnable2.IsChecked ?? false,
                   int.Parse(chartControl.MaPeriodText2.Text),
                   (MaTypeModel)chartControl.MaTypeCombo2.SelectedItem,
                   (LineColorModel)chartControl.MaLineColorCombo2.SelectedItem,
                   (LineWeightModel)chartControl.MaLineWeightCombo2.SelectedItem));
                SettingsMan.Indicators.Mas.Add(new MaModel(
                   chartControl.MaEnable3.IsChecked ?? false,
                   int.Parse(chartControl.MaPeriodText3.Text),
                   (MaTypeModel)chartControl.MaTypeCombo3.SelectedItem,
                   (LineColorModel)chartControl.MaLineColorCombo3.SelectedItem,
                   (LineWeightModel)chartControl.MaLineWeightCombo3.SelectedItem));
                SettingsMan.Indicators.Mas.Add(new MaModel(
                   chartControl.MaEnable4.IsChecked ?? false,
                   int.Parse(chartControl.MaPeriodText4.Text),
                   (MaTypeModel)chartControl.MaTypeCombo4.SelectedItem,
                   (LineColorModel)chartControl.MaLineColorCombo4.SelectedItem,
                   (LineWeightModel)chartControl.MaLineWeightCombo4.SelectedItem));
                SettingsMan.Indicators.Mas.Add(new MaModel(
                   chartControl.MaEnable5.IsChecked ?? false,
                   int.Parse(chartControl.MaPeriodText5.Text),
                   (MaTypeModel)chartControl.MaTypeCombo5.SelectedItem,
                   (LineColorModel)chartControl.MaLineColorCombo5.SelectedItem,
                   (LineWeightModel)chartControl.MaLineWeightCombo5.SelectedItem));
                SettingsMan.Indicators.Bbs.Clear();
                SettingsMan.Indicators.Bbs.Add(new BbModel(
                    chartControl.BbEnable1.IsChecked ?? false,
                    int.Parse(chartControl.BbPeriodText1.Text),
                    int.Parse(chartControl.BbDeviationText1.Text),
                    (LineColorModel)chartControl.BbSmaLineColorCombo1.SelectedItem,
                    (LineWeightModel)chartControl.BbSmaLineWeightCombo1.SelectedItem,
                    (LineColorModel)chartControl.BbUpperLineColorCombo1.SelectedItem,
                    (LineWeightModel)chartControl.BbUpperLineWeightCombo1.SelectedItem,
                    (LineColorModel)chartControl.BbLowerLineColorCombo1.SelectedItem,
                    (LineWeightModel)chartControl.BbLowerLineWeightCombo1.SelectedItem));

                // Market
                SettingsMan.BinanceApiKey = marketControl.BinanceApiKeyText.Text;
                SettingsMan.BinanceSecretKey = marketControl.BinanceApiSecretKeyText.Text;
                SettingsMan.UpbitApiKey = marketControl.UpbitApiKeyText.Text;
                SettingsMan.UpbitSecretKey = marketControl.UpbitApiSecretKeyText.Text;
                SettingsMan.BithumbApiKey = marketControl.BithumbApiKeyText.Text;
                SettingsMan.BithumbSecretKey = marketControl.BithumbApiSecretKeyText.Text;

                // Pair
                SettingsMan.SimpleListCount = int.Parse(pairControl.SimpleListCountText.Text);

                SettingsMan.Save();
            }
            catch (Exception ex)
            {
                Logger.Log(nameof(SettingsView), MethodBase.GetCurrentMethod()?.Name, "#설정값에러#" + ex.ToString());
                MessageBox.Show("설정값 중에서 잘못된 값이 있습니다.\n다시 한번 확인해 주세요.");
            }

            try
            {
                Common.CalculateIndicators?.Invoke();
            }
            catch (Exception ex)
            {
                Logger.Log(nameof(SettingsView), MethodBase.GetCurrentMethod()?.Name, ex.ToString());
            }
        }
    }
}
