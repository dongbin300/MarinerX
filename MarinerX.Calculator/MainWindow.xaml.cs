using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace MarinerX.Calculator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            LeverageText.Text = "10";
            MaxActiveDealsText.Text = "10";
            BaseOrderText.Text = "30";
            SafetyOrderText.Text = "60";
            MaxSafetyOrderCountText.Text = "3";
            SafetyOrderVolumeScaleText.Text = "1.5";
        }

        private void TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                var leverage = int.Parse(LeverageText.Text);
                var maxActiveDeals = int.Parse(MaxActiveDealsText.Text);
                var baseOrder = double.Parse(BaseOrderText.Text);
                var safetyOrder = double.Parse(SafetyOrderText.Text);
                var maxSafetyOrderCount = int.Parse(MaxSafetyOrderCountText.Text);
                var safetyOrderVolumeScale = double.Parse(SafetyOrderVolumeScaleText.Text);

                var orderSizes = new List<double> { baseOrder };
                for (int i = 0; i < maxSafetyOrderCount; i++)
                {
                    orderSizes.Add(safetyOrder * Math.Pow(safetyOrderVolumeScale, i));
                }

                var result = (int)(orderSizes.Sum() * maxActiveDeals / leverage);
                RequireAssetText.Text = result + " USDT";
            }
            catch
            {
                RequireAssetText.Text = "- USDT";
            }
        }
    }
}
