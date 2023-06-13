using CryptoModel;
using CryptoModel.Backtests;
using CryptoModel.Charts;

using MarinerXX.Views;

using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace MarinerXX
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<SimpleDealManager> dealResult = new();

        public MainWindow()
        {
            InitializeComponent();

            SymbolTextBox.Text = Settings.Default.Symbol;
            StartDateTextBox.Text = Settings.Default.StartDate;
            EndDateTextBox.Text = Settings.Default.EndDate;
            FileNameTextBox.Text = Settings.Default.FileName;
        }

        private void BacktestButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Settings.Default.Symbol = SymbolTextBox.Text;
                Settings.Default.StartDate = StartDateTextBox.Text;
                Settings.Default.EndDate = EndDateTextBox.Text;
                Settings.Default.FileName = FileNameTextBox.Text;
                Settings.Default.Save();

                var p1 = Parameter1TextBox.Text.ToDecimal();
                var p2 = Parameter2TextBox.Text.ToDecimal();
                var p3 = Parameter3TextBox.Text.ToDecimal();

                dealResult.Clear();
                var symbols = SymbolTextBox.Text.Split(';');
                var interval = ((IntervalComboBox.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "5m").ToKlineInterval();

                BacktestProgress.Value = 0;
                BacktestProgress.Maximum = symbols.Length;
                foreach (var symbol in symbols)
                {
                    try
                    {
                        BacktestProgress.Value++;
                        var startDate = StartDateTextBox.Text.ToDateTime() > CryptoSymbol.GetStartDate(symbol).AddDays(1) ? StartDateTextBox.Text.ToDateTime() : CryptoSymbol.GetStartDate(symbol).AddDays(1);
                        var endDate = EndDateTextBox.Text.ToDateTime();

                        if (startDate >= endDate)
                        {
                            continue;
                        }

                        // 차트 로드 및 초기화
                        if (ChartLoader.GetChartPack(symbol, interval) == null)
                        {
                            ChartLoader.InitChartsByDate(symbol, interval, startDate, endDate);
                        }

                        // 차트 진행하면서 매매
                        var charts = ChartLoader.GetChartPack(symbol, interval);

                        StrategyEv(charts, p1);
                    }
                    catch (FileNotFoundException)
                    {
                        continue;
                    }
                }

                foreach (var d in dealResult)
                {
                    var content = $"{d.ChartInfo.Symbol},{d.TargetRoe},{d.TotalIncome.Round(2)},{d.BacktestDays},{d.IncomePerDay.Round(2)}" + Environment.NewLine;
                    File.AppendAllText(CryptoPath.Desktop.Down($"{FileNameTextBox.Text}.csv"), content);

                    if (dealResult.Count == 1)
                    {
                        var resultView = new BacktestResultView();
                        resultView.Init(symbols[0], d);
                        resultView.Show();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void StrategyEv(ChartPack charts, decimal p1)
        {
            charts.CalculateIndicatorsEveryonesCoin();

            var dealManager = new SimpleDealManager(charts.Charts[0].DateTime, charts.Charts[^1].DateTime, 100, 1.75m);
            for (int i = 1; i < charts.Charts.Count; i++)
            {
                if (p1 == 0)
                {
                    dealManager.EvaluateEveryonesCoinShort(charts.Charts[i], charts.Charts[i - 1]);
                }
                else if (p1 == 1)
                {
                    dealManager.EvaluateEveryonesCoinLong(charts.Charts[i], charts.Charts[i - 1]);
                }
            }

            // Set latest chart for UPNL
            dealManager.ChartInfo = charts.Charts[^1];
            dealResult.Add(dealManager);
        }

        private void StrategyStefano(ChartPack charts)
        {
            charts.CalculateIndicatorsStefano();

            var dealManager = new SimpleDealManager(charts.Charts[0].DateTime, charts.Charts[^1].DateTime, 100, null, 2.0m);
            for (int i = 1; i < charts.Charts.Count; i++)
            {
                dealManager.EvaluateStefanoLong(charts.Charts[i], charts.Charts[i - 1], 0.2m, 0.5m);
            }
            dealManager.ChartInfo = charts.Charts[^1];
            dealResult.Add(dealManager);
        }
    }
}
