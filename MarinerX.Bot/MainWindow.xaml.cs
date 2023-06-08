using MarinerX.Bot.Bots;
using MarinerX.Bot.Clients;
using MarinerX.Bot.Models;
using MarinerX.Bot.Systems;

using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace MarinerX.Bot
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DispatcherTimer timer = new();
        DispatcherTimer timer1m = new();
        ManagerBot manager = new("매니저 봇", "심볼 모니터링, 포지션 모니터링, 자산 모니터링 등등 전반적인 시스템을 관리하는 봇입니다.");
        ChartBot chart = new("차트 봇", "차트와 관련된 계산을 하는 봇입니다.");
        LongBot longPosition = new("롱 봇", "롱 포지션 매매를 하는 봇입니다.");

        public MainWindow()
        {
            InitializeComponent();
            Init();
        }

        private async void Init()
        {
            if (!Directory.Exists("Logs"))
            {
                Directory.CreateDirectory("Logs");
            }

            Common.LoadSymbolDetail();

            BinanceClients.Init();

            Account.AddHistory = (text) =>
            {
                DispatcherService.Invoke(() =>
                {
                    HistoryDataGrid.Items.Add(new BotHistory(DateTime.Now, text));
                });
            };

            await manager.GetAllKlines().ConfigureAwait(false);
            await manager.StartBinanceFuturesTicker().ConfigureAwait(false);

            timer.Interval = TimeSpan.FromMilliseconds(1000);
            timer.Tick += Timer_Tick;

            timer1m.Interval = TimeSpan.FromMinutes(1);
            timer1m.Tick += Timer1m_Tick;
        }

        private async void Timer1m_Tick(object? sender, EventArgs e)
        {
            try
            {
                await Task.Delay(5);

                /* 주문 모니터링 - 5분이 넘도록 체결이 안되는 주문 취소 */
                //await longPosition.MonitorOpenOrderTimeout().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Logger.Log(nameof(MainWindow), MethodBase.GetCurrentMethod()?.Name, ex);
            }
        }

        private async void Timer_Tick(object? sender, EventArgs e)
        {
            try
            {
                /* 포지션 모니터링 */
                await manager.GetBinancePositions().ConfigureAwait(false);
                //DispatcherService.Invoke(() =>
                //{
                //    PositionDataGrid.ItemsSource = Account.Positions;
                //});

                /* 자산 모니터링 */
                //(var total, var avbl, var bnb) = await manager.GetBinanceBalance();
                //BalanceText.Text = $"{total} USDT";
                //BnbText.Text = $"{bnb} BNB";

                /* 수익 모니터링 */
                //var todayRealizedPnlHistory = await manager.GetBinanceTodayRealizedPnlHistory();
                //TodayPnlText.ToolTip = new TextBlock() { Text = string.Join(Environment.NewLine, todayRealizedPnlHistory.Select(x => x.ToString())), Foreground = new SolidColorBrush(Colors.Black) };
                //var todayPnl = Math.Round(todayRealizedPnlHistory.Sum(x => x.RealizedPnl), 3);
                //if (todayPnl >= 0)
                //{
                //    TodayPnlText.Foreground = Common.LongColor;
                //    TodayPnlText.Text = $"+{todayPnl} USDT";
                //}
                //else
                //{
                //    TodayPnlText.Foreground = Common.ShortColor;
                //    TodayPnlText.Text = $"{todayPnl} USDT";
                //}

                //await longPosition.Evaluate().ConfigureAwait(false);
                await longPosition.MockEvaluate().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Logger.Log(nameof(MainWindow), MethodBase.GetCurrentMethod()?.Name, ex);
            }
        }

        private void MockBotCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                longPosition.BaseOrderSize = int.Parse(BaseOrderSizeTextBox.Text);
                longPosition.TargetRoe = decimal.Parse(TargetProfitTextBox.Text);
                longPosition.Leverage = int.Parse(LeverageTextBox.Text);
                longPosition.MaxActiveDeals = int.Parse(MaxActiveDealsTextBox.Text);

                timer.Start();

                Account.AddHistory("Mock Bot On");
            }
            catch (Exception ex)
            {
                Logger.Log(nameof(MainWindow), MethodBase.GetCurrentMethod()?.Name, ex);
            }
        }

        private void MockBotCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                timer.Stop();

                Account.AddHistory("Mock Bot Off");
            }
            catch (Exception ex)
            {
                Logger.Log(nameof(MainWindow), MethodBase.GetCurrentMethod()?.Name, ex);
            }
        }
    }
}
