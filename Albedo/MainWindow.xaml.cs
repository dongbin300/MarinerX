using Albedo.Models;
using Albedo.Utils;
using Albedo.Views;

using Binance.Net.Clients;
using Binance.Net.Interfaces;
using Binance.Net.Objects;

using CryptoExchange.Net.Sockets;

using MercuryTradingModel.Quotes;

using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace Albedo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        BinanceSocketClient binanceClient = new();
        System.Timers.Timer timer = new System.Timers.Timer(1000);

        public MainWindow()
        {
            InitializeComponent();
            InitBinanceClient();
            binanceClient.UsdFuturesStreams.SubscribeToAllTickerUpdatesAsync(BinanceAllTickerUpdates);
            timer.Elapsed += Timer_Elapsed;
            timer.Start();
        }

        private void Timer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            RefreshSymbolList();
        }

        void InitBinanceClient()
        {
            try
            {
                var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Gaten", "binance_api.txt");
                var data = File.ReadAllLines(path);

                binanceClient = new BinanceSocketClient(new BinanceSocketClientOptions
                {
                    ApiCredentials = new BinanceApiCredentials(data[0], data[1])
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        void BinanceAllTickerUpdates(DataEvent<IEnumerable<IBinance24HPrice>> obj)
        {
            var data = obj.Data;
            foreach(var item in data)
            {
                Menu.viewModel.UpdateSymbolInfo(new Symbol("binance", item.Symbol, item.LastPrice, item.PriceChangePercent));
            }
        }

        public void RefreshSymbolList()
        {
            DispatcherService.Invoke(() =>
            {
                Menu.MainGrid.RowDefinitions.Clear();
                Menu.MainGrid.Children.Clear();
                for (int i = 0; i < Menu.viewModel.Symbols.Count; i++)
                {
                    Menu.MainGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(45) });
                    var symbol = Menu.viewModel.Symbols[i];

                    var symbolControl = new SymbolControl();
                    symbolControl.Init(symbol);

                    /* 해당 심볼메뉴 클릭 */
                    symbolControl.SymbolClick = (_symbol) =>
                    {
                        var chartControl = new ChartControl();
                        chartControl.Init(QuoteUtil.GetQuotesFromLocal("BTCUSDT", DateTime.Parse("2023-03-16")));
                        Chart.Content = chartControl;
                    };

                    symbolControl.SetValue(Grid.RowProperty, i);
                    Menu.MainGrid.Children.Add(symbolControl);
                }
            });
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}
