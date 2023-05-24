using Albedo.Managers;
using Albedo.Views.Settings;
using Albedo.Models;

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
            SettingsMan.Indicators.Mas.Clear();
            SettingsMan.Indicators.Mas.Add(new MaModel(
                chartControl.MaEnable1.IsChecked ?? false,
                int.Parse(chartControl.MaPeriodText1.Text),
                (MaTypeModel)chartControl.MaTypeCombo1.SelectedItem,
                (LineColorModel)chartControl.MaLineColorCombo1.SelectedItem,
                (LineWeightModel)chartControl.MaLineWeightCombo1.SelectedItem));
            SettingsMan.Save();

            Common.CalculateIndicators?.Invoke();
        }
    }
}
