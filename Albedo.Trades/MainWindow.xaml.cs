using Albedo.Trades.Models;

using Binance.Net.Clients;
using Binance.Net.Objects;

using System;
using System.Windows;
using System.Windows.Threading;

namespace Albedo.Trades
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        BinanceClient client = default!;
        BinanceSocketClient socketClient = default!;

        void Invoke(Action action) => Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, action);

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            client = new BinanceClient(new BinanceClientOptions
            {
                // API Key 없어도 잘 돌아감?
                //ApiCredentials = new BinanceApiCredentials(Settings.Default.BinanceApiKey, Settings.Default.BinanceSecretKey)
            });

            socketClient = new BinanceSocketClient(new BinanceSocketClientOptions
            {
                // API Key 없어도 잘 돌아감?
                //ApiCredentials = new BinanceApiCredentials(Settings.Default.BinanceApiKey, Settings.Default.BinanceSecretKey)
            });

            var result = client.UsdFuturesApi.ExchangeData.GetExchangeInfoAsync();
            result.Wait();
            SymbolComboBox.ItemsSource = result.Result.Data.Symbols;
        }

        private void SymbolComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var symbol = SymbolComboBox.SelectedValue.ToString();
            if (symbol == null)
            {
                return;
            }

            TradesDataGrid.Items.Clear();

            socketClient.UsdFuturesStreams.UnsubscribeAllAsync();
            socketClient.UsdFuturesStreams.SubscribeToTradeUpdatesAsync(symbol, (obj) =>
            {
                var price = obj.Data.Price;
                var quantity = obj.Data.Quantity;
                var buyerIsMaker = obj.Data.BuyerIsMaker;

                var trade = new BinanceTrade(price, quantity, buyerIsMaker);

                Invoke(() =>
                {
                    if (!decimal.TryParse(AmountFilterTextBox.Text, out var amountFilter))
                    {
                        amountFilter = 100;
                    }

                    if (!decimal.TryParse(HighlightFilterTextBox.Text, out var highlightFilter))
                    {
                        highlightFilter = 10000;
                    }

                    if (trade.Amount >= amountFilter)
                    {
                        trade.SetHighlight(highlightFilter);
                        TradesDataGrid.Items.Insert(0, trade);
                    }
                });
            });
        }
    }
}
