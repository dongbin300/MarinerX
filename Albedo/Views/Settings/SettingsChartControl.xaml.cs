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
            MaTypeCombo2.Items.Clear();
            MaTypeCombo3.Items.Clear();
            MaTypeCombo4.Items.Clear();
            MaTypeCombo5.Items.Clear();
            foreach (var item in Common.MaTypes)
            {
                MaTypeCombo1.Items.Add(item);
                MaTypeCombo2.Items.Add(item);
                MaTypeCombo3.Items.Add(item);
                MaTypeCombo4.Items.Add(item);
                MaTypeCombo5.Items.Add(item);
            }

            MaLineColorCombo1.Items.Clear();
            MaLineColorCombo2.Items.Clear();
            MaLineColorCombo3.Items.Clear();
            MaLineColorCombo4.Items.Clear();
            MaLineColorCombo5.Items.Clear();
            BbSmaLineColorCombo1.Items.Clear();
            BbUpperLineColorCombo1.Items.Clear();
            BbLowerLineColorCombo1.Items.Clear();
            foreach (var item in Common.MaLineColors)
            {
                MaLineColorCombo1.Items.Add(item);
                MaLineColorCombo2.Items.Add(item);
                MaLineColorCombo3.Items.Add(item);
                MaLineColorCombo4.Items.Add(item);
                MaLineColorCombo5.Items.Add(item);
                BbSmaLineColorCombo1.Items.Add(item);
                BbUpperLineColorCombo1.Items.Add(item);
                BbLowerLineColorCombo1.Items.Add(item);
            }

            MaLineWeightCombo1.Items.Clear();
            MaLineWeightCombo2.Items.Clear();
            MaLineWeightCombo3.Items.Clear();
            MaLineWeightCombo4.Items.Clear();
            MaLineWeightCombo5.Items.Clear();
            BbSmaLineWeightCombo1.Items.Clear();
            BbUpperLineWeightCombo1.Items.Clear();
            BbLowerLineWeightCombo1.Items.Clear();
            foreach (var item in Common.MaLineWeights)
            {
                MaLineWeightCombo1.Items.Add(item);
                MaLineWeightCombo2.Items.Add(item);
                MaLineWeightCombo3.Items.Add(item);
                MaLineWeightCombo4.Items.Add(item);
                MaLineWeightCombo5.Items.Add(item);
                BbSmaLineWeightCombo1.Items.Add(item);
                BbUpperLineWeightCombo1.Items.Add(item);
                BbLowerLineWeightCombo1.Items.Add(item);
            }

            DefaultCandleCountErrorText.Visibility = Visibility.Hidden;
            isInit = true;
            LoadSettings();
            isInit = false;
        }

        private void LoadSettings()
        {
            DefaultCandleCountText.Text = SettingsMan.DefaultCandleCount.ToString();

            // 기본값
            MaPeriodText1.Text = "5";
            MaPeriodText2.Text = "10";
            MaPeriodText3.Text = "20";
            MaPeriodText4.Text = "60";
            MaPeriodText5.Text = "120";
            MaTypeCombo1.SelectedIndex = 0;
            MaTypeCombo2.SelectedIndex = 0;
            MaTypeCombo3.SelectedIndex = 0;
            MaTypeCombo4.SelectedIndex = 0;
            MaTypeCombo5.SelectedIndex = 0;
            MaLineColorCombo1.SelectedIndex = 0;
            MaLineColorCombo2.SelectedIndex = 1;
            MaLineColorCombo3.SelectedIndex = 2;
            MaLineColorCombo4.SelectedIndex = 3;
            MaLineColorCombo5.SelectedIndex = 4;
            MaLineWeightCombo1.SelectedIndex = 0;
            MaLineWeightCombo2.SelectedIndex = 0;
            MaLineWeightCombo3.SelectedIndex = 0;
            MaLineWeightCombo4.SelectedIndex = 0;
            MaLineWeightCombo5.SelectedIndex = 0;

            BbPeriodText1.Text = "20";
            BbDeviationText1.Text = "2";
            BbSmaLineColorCombo1.SelectedIndex = 0;
            BbUpperLineColorCombo1.SelectedIndex = 0;
            BbLowerLineColorCombo1.SelectedIndex = 0;
            BbSmaLineWeightCombo1.SelectedIndex = 0;
            BbUpperLineWeightCombo1.SelectedIndex = 0;
            BbLowerLineWeightCombo1.SelectedIndex = 0;

            #region MA
            // 이평선 1
            if (SettingsMan.Indicators.Mas.Count >= 1)
            {
                MaEnable1.IsChecked = SettingsMan.Indicators.Mas[0].Enable;
                MaPeriodText1.Text = SettingsMan.Indicators.Mas[0].Period.ToString();
                MaTypeCombo1.SelectedItem = MaTypeCombo1.Items.OfType<MaTypeModel>().First(x => x.Type.Equals(SettingsMan.Indicators.Mas[0].Type.Type));
                MaLineColorCombo1.SelectedItem = MaLineColorCombo1.Items.OfType<LineColorModel>().First(x => x.LineColor.Equals(SettingsMan.Indicators.Mas[0].LineColor.LineColor));
                MaLineWeightCombo1.SelectedItem = MaLineWeightCombo1.Items.OfType<LineWeightModel>().First(x => x.LineWeight.Equals(SettingsMan.Indicators.Mas[0].LineWeight.LineWeight));
            }

            // 이평선 2
            if (SettingsMan.Indicators.Mas.Count >= 2)
            {
                MaEnable2.IsChecked = SettingsMan.Indicators.Mas[1].Enable;
                MaPeriodText2.Text = SettingsMan.Indicators.Mas[1].Period.ToString();
                MaTypeCombo2.SelectedItem = MaTypeCombo2.Items.OfType<MaTypeModel>().First(x => x.Type.Equals(SettingsMan.Indicators.Mas[1].Type.Type));
                MaLineColorCombo2.SelectedItem = MaLineColorCombo2.Items.OfType<LineColorModel>().First(x => x.LineColor.Equals(SettingsMan.Indicators.Mas[1].LineColor.LineColor));
                MaLineWeightCombo2.SelectedItem = MaLineWeightCombo2.Items.OfType<LineWeightModel>().First(x => x.LineWeight.Equals(SettingsMan.Indicators.Mas[1].LineWeight.LineWeight));
            }

            // 이평선 3
            if (SettingsMan.Indicators.Mas.Count >= 3)
            {
                MaEnable3.IsChecked = SettingsMan.Indicators.Mas[2].Enable;
                MaPeriodText3.Text = SettingsMan.Indicators.Mas[2].Period.ToString();
                MaTypeCombo3.SelectedItem = MaTypeCombo3.Items.OfType<MaTypeModel>().First(x => x.Type.Equals(SettingsMan.Indicators.Mas[2].Type.Type));
                MaLineColorCombo3.SelectedItem = MaLineColorCombo3.Items.OfType<LineColorModel>().First(x => x.LineColor.Equals(SettingsMan.Indicators.Mas[2].LineColor.LineColor));
                MaLineWeightCombo3.SelectedItem = MaLineWeightCombo3.Items.OfType<LineWeightModel>().First(x => x.LineWeight.Equals(SettingsMan.Indicators.Mas[2].LineWeight.LineWeight));
            }

            // 이평선 4
            if (SettingsMan.Indicators.Mas.Count >= 4)
            {
                MaEnable4.IsChecked = SettingsMan.Indicators.Mas[3].Enable;
                MaPeriodText4.Text = SettingsMan.Indicators.Mas[3].Period.ToString();
                MaTypeCombo4.SelectedItem = MaTypeCombo4.Items.OfType<MaTypeModel>().First(x => x.Type.Equals(SettingsMan.Indicators.Mas[3].Type.Type));
                MaLineColorCombo4.SelectedItem = MaLineColorCombo4.Items.OfType<LineColorModel>().First(x => x.LineColor.Equals(SettingsMan.Indicators.Mas[3].LineColor.LineColor));
                MaLineWeightCombo4.SelectedItem = MaLineWeightCombo4.Items.OfType<LineWeightModel>().First(x => x.LineWeight.Equals(SettingsMan.Indicators.Mas[3].LineWeight.LineWeight));
            }

            // 이평선 5
            if (SettingsMan.Indicators.Mas.Count >= 5)
            {
                MaEnable5.IsChecked = SettingsMan.Indicators.Mas[4].Enable;
                MaPeriodText5.Text = SettingsMan.Indicators.Mas[4].Period.ToString();
                MaTypeCombo5.SelectedItem = MaTypeCombo5.Items.OfType<MaTypeModel>().First(x => x.Type.Equals(SettingsMan.Indicators.Mas[4].Type.Type));
                MaLineColorCombo5.SelectedItem = MaLineColorCombo5.Items.OfType<LineColorModel>().First(x => x.LineColor.Equals(SettingsMan.Indicators.Mas[4].LineColor.LineColor));
                MaLineWeightCombo5.SelectedItem = MaLineWeightCombo5.Items.OfType<LineWeightModel>().First(x => x.LineWeight.Equals(SettingsMan.Indicators.Mas[4].LineWeight.LineWeight));
            }
            #endregion

            #region BB
            // 볼린저밴드 1
            if (SettingsMan.Indicators.Bbs.Count >= 1)
            {
                BbEnable1.IsChecked = SettingsMan.Indicators.Bbs[0].Enable;
                BbPeriodText1.Text = SettingsMan.Indicators.Bbs[0].Period.ToString();
                BbDeviationText1.Text = SettingsMan.Indicators.Bbs[0].Deviation.ToString();
                BbSmaLineColorCombo1.SelectedItem = BbSmaLineColorCombo1.Items.OfType<LineColorModel>().First(x => x.LineColor.Equals(SettingsMan.Indicators.Bbs[0].SmaLineColor.LineColor));
                BbUpperLineColorCombo1.SelectedItem = BbUpperLineColorCombo1.Items.OfType<LineColorModel>().First(x => x.LineColor.Equals(SettingsMan.Indicators.Bbs[0].UpperLineColor.LineColor));
                BbLowerLineColorCombo1.SelectedItem = BbLowerLineColorCombo1.Items.OfType<LineColorModel>().First(x => x.LineColor.Equals(SettingsMan.Indicators.Bbs[0].LowerLineColor.LineColor));
                BbSmaLineWeightCombo1.SelectedItem = BbSmaLineWeightCombo1.Items.OfType<LineWeightModel>().First(x => x.LineWeight.Equals(SettingsMan.Indicators.Bbs[0].SmaLineWeight.LineWeight));
                BbUpperLineWeightCombo1.SelectedItem = BbUpperLineWeightCombo1.Items.OfType<LineWeightModel>().First(x => x.LineWeight.Equals(SettingsMan.Indicators.Bbs[0].UpperLineWeight.LineWeight));
                BbLowerLineWeightCombo1.SelectedItem = BbLowerLineWeightCombo1.Items.OfType<LineWeightModel>().First(x => x.LineWeight.Equals(SettingsMan.Indicators.Bbs[0].LowerLineWeight.LineWeight));
            }
            #endregion
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
                }
                else
                {
                    DefaultCandleCountErrorText.Visibility = Visibility.Visible;
                }
            }
            else
            {
                DefaultCandleCountErrorText.Visibility = Visibility.Visible;
            }
        }
    }
}
