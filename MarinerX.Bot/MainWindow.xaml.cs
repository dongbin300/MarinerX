using CryptoModel;

using MarinerX.Bot.Bots;
using MarinerX.Bot.Clients;
using MarinerX.Bot.Models;
using MarinerX.Bot.Systems;

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
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
        DispatcherTimer timer1m = new();
        ManagerBot manager = new("매니저 봇", "심볼 모니터링, 포지션 모니터링, 자산 모니터링 등등 전반적인 시스템을 관리하는 봇입니다.");
        ChartBot chart = new("차트 봇", "차트와 관련된 계산을 하는 봇입니다.");
        LongBot longPosition = new("롱 봇", "롱 포지션 매매를 하는 봇입니다.");
        ShortBot shortPosition = new("숏 봇", "숏 포지션 매매를 하는 봇입니다.");

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

            // 봇 히스토리 추가
            Common.AddHistory = (subject, text) =>
            {
                DispatcherService.Invoke(() =>
                {
                    var history = new BotHistory(DateTime.Now, subject, text);
                    HistoryDataGrid.Items.Add(history);
                    Logger.LogHistory(history);
                });
            };

            await manager.GetAllKlines().ConfigureAwait(false);
            await manager.StartBinanceFuturesTicker().ConfigureAwait(false);

            timer.Interval = TimeSpan.FromMilliseconds(1000);
            timer.Tick += Timer_Tick;
            timer.Start();

            //timer1m.Interval = TimeSpan.FromMinutes(1);
            //timer1m.Tick += Timer1m_Tick;
        }

        private async void Timer1m_Tick(object? sender, EventArgs e)
        {
            try
            {
                await Task.Delay(5);

                /* 주문 모니터링 - 5분이 넘도록 체결이 안되는 주문 취소 (이 부분은 좀 더 테스트 필요) */
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
                DispatcherService.Invoke(() =>
                {
                    PositionDataGrid.ItemsSource = Common.Positions;
                });

                /* 자산 모니터링 */
                (var total, var avbl, var bnb) = await manager.GetBinanceBalance();
                DispatcherService.Invoke(() =>
                {
                    BalanceText.Text = $"{total} USDT";
                    BnbText.Text = $"{bnb} BNB";
                });

                /* 수익 모니터링 */
                var todayRealizedPnlHistory = await manager.GetBinanceTodayRealizedPnlHistory();
                DispatcherService.Invoke(() =>
                {
                    if (todayRealizedPnlHistory != null && todayRealizedPnlHistory.Count(x => x == null) == 0)
                    {
                        TodayPnlText.ToolTip = new TextBlock() { Text = string.Join(Environment.NewLine, todayRealizedPnlHistory.Select(x => x.ToString())), Foreground = new SolidColorBrush(Colors.Black) };
                        var todayPnl = Math.Round(todayRealizedPnlHistory.Sum(x => x.RealizedPnl), 3);
                        if (todayPnl >= 0)
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
                });

                if (longPosition.IsRunning)
                {
                    await longPosition.Evaluate().ConfigureAwait(false);
                }

                if (shortPosition.IsRunning)
                {
                    await shortPosition.Evaluate().ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                Logger.Log(nameof(MainWindow), MethodBase.GetCurrentMethod()?.Name, ex);
            }
        }

        #region Mock
        private void MockBotCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                longPosition.BaseOrderSize = int.Parse(BaseOrderSizeTextBox.Text);
                longPosition.TargetRoe = decimal.Parse(TargetProfitTextBox.Text);
                longPosition.Leverage = int.Parse(LeverageTextBox.Text);
                longPosition.MaxActiveDeals = int.Parse(MaxActiveDealsTextBox.Text);

                timer.Start();

                Common.AddHistory("Master", "Mock Bot On");
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

                Common.AddHistory("Master", "Mock Bot Off");
            }
            catch (Exception ex)
            {
                Logger.Log(nameof(MainWindow), MethodBase.GetCurrentMethod()?.Name, ex);
            }
        }
        #endregion

        private void LongBotCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                longPosition.BaseOrderSize = int.Parse(BaseOrderSizeTextBox.Text);
                longPosition.TargetRoe = decimal.Parse(TargetProfitTextBox.Text);
                longPosition.Leverage = int.Parse(LeverageTextBox.Text);
                longPosition.MaxActiveDeals = int.Parse(MaxActiveDealsTextBox.Text);

                Common.AddHistory("Master", "Long Bot On");
                longPosition.IsRunning = true;
            }
            catch (Exception ex)
            {
                Logger.Log(nameof(MainWindow), MethodBase.GetCurrentMethod()?.Name, ex);
            }
        }

        private void LongBotCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                Common.AddHistory("Master", "Long Bot Off");
                longPosition.IsRunning = false;
            }
            catch (Exception ex)
            {
                Logger.Log(nameof(MainWindow), MethodBase.GetCurrentMethod()?.Name, ex);
            }
        }

        private void ShortBotCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                shortPosition.BaseOrderSize = int.Parse(BaseOrderSizeTextBox.Text);
                shortPosition.TargetRoe = decimal.Parse(TargetProfitTextBox.Text);
                shortPosition.Leverage = int.Parse(LeverageTextBox.Text);
                shortPosition.MaxActiveDeals = int.Parse(MaxActiveDealsTextBox.Text);

                Common.AddHistory("Master", "Short Bot On");
                shortPosition.IsRunning = true;
            }
            catch (Exception ex)
            {
                Logger.Log(nameof(MainWindow), MethodBase.GetCurrentMethod()?.Name, ex);
            }
        }

        private void ShortBotCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                Common.AddHistory("Master", "Short Bot Off");
                shortPosition.IsRunning = false;
            }
            catch (Exception ex)
            {
                Logger.Log(nameof(MainWindow), MethodBase.GetCurrentMethod()?.Name, ex);
            }
        }

        private int CalculateRequireAsset()
        {
            try
            {
                if (MaxActiveDealsTextBox == null || BaseOrderSizeTextBox == null || LeverageTextBox == null)
                {
                    return 0;
                }

                return (int)(MaxActiveDealsTextBox.Text.ToInt() * BaseOrderSizeTextBox.Text.ToDecimal() / LeverageTextBox.Text.ToInt());
            }
            catch
            {
                return 0;
            }
        }

        private void BaseOrderSizeTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (RequireAssetText == null)
            {
                return;
            }

            RequireAssetText.Text = $"{CalculateRequireAsset():N} USDT";
        }

        private void LeverageTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (RequireAssetText == null)
            {
                return;
            }

            RequireAssetText.Text = $"{CalculateRequireAsset():N} USDT";
        }

        private void MaxActiveDealsTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (RequireAssetText == null)
            {
                return;
            }

            RequireAssetText.Text = $"{CalculateRequireAsset():N} USDT";
        }
    }
}
