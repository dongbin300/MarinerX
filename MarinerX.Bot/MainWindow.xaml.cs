using MarinerX.Bot.Managers;
using MarinerX.Bot.Models;

using System;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace MarinerX.Bot
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DispatcherTimer timer = new();
        BinanceManager b = new BinanceManager();

        public MainWindow()
        {
            InitializeComponent();

            timer.Interval = TimeSpan.FromMilliseconds(1000);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private async void Timer_Tick(object? sender, EventArgs e)
        {
            try
            {
                var _positions = await b.GetBinancePositions();
                var positions = _positions.Select(p => new BinancePosition(
                    p.Symbol,
                    p.PositionSide.ToString(),
                    Math.Round(Math.Abs(p.MarkPrice * p.Quantity / p.Leverage), 3).ToString(),
                    (p.UnrealizedPnl >= 0 ? "+" + Math.Round(p.UnrealizedPnl, 3).ToString() : Math.Round(p.UnrealizedPnl, 3).ToString())
                    + ((p.UnrealizedPnl / Math.Abs(p.MarkPrice * p.Quantity / p.Leverage)) >= 0 ? $" (+{Math.Round(p.UnrealizedPnl / Math.Abs(p.MarkPrice * p.Quantity / p.Leverage) * 100, 2)}%)" : $" ({Math.Round(p.UnrealizedPnl / Math.Abs(p.MarkPrice * p.Quantity / p.Leverage) * 100, 2)}%)")));
                PositionDataGrid.ItemsSource = positions;

                (var total, var avbl, var bnb) = await b.GetBinanceBalance();
                BalanceText.Text = $"{total} USDT";
                BnbText.Text = $"{bnb} BNB";

                var todayRealizedPnlHistory = await b.GetBinanceTodayRealizedPnlHistory();
                TodayPnlText.ToolTip = new TextBlock() { Text = string.Join(Environment.NewLine, todayRealizedPnlHistory.Select(x => x.ToString())), Foreground = new SolidColorBrush(Colors.Black) };
                var todayPnl = Math.Round(todayRealizedPnlHistory.Sum(x => x.RealizedPnl), 3);
                if(todayPnl >= 0)
                {
                    TodayPnlText.Foreground = Common.LongColor;
                    TodayPnlText.Text = $"+{todayPnl} USDT";
                }
                else
                {
                    TodayPnlText.Foreground = Common.ShortColor;
                    TodayPnlText.Text = $"{todayPnl} USDT";
                }
            }
            catch (Exception ex)
            {
                Logger.Log(nameof(MainWindow), MethodBase.GetCurrentMethod()?.Name, ex);
            }
        }
    }
}
