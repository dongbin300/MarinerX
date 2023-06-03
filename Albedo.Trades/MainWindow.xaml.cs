using Albedo.Trades.Models;

using Binance.Net.Clients;
using Binance.Net.Objects;

using MercuryTradingModel.Extensions;

using System;
using System.IO;
using System.Linq;
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
        BinanceSocketClient socketClient2 = default!;

        void Invoke(Action action) => Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, action);

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var data = File.ReadAllLines(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).Down("Gaten", "binance_api.txt"));
            client = new BinanceClient();
            socketClient = new BinanceSocketClient();
            socketClient2 = new BinanceSocketClient(new BinanceSocketClientOptions
            {
                ApiCredentials = new BinanceApiCredentials(data[0], data[1])
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
            socketClient2.UsdFuturesStreams.UnsubscribeAllAsync();

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

            socketClient2.UsdFuturesStreams.SubscribeToAllBookTickerUpdatesAsync((obj) =>
            {
            });
            socketClient2.UsdFuturesStreams.SubscribeToBookTickerUpdatesAsync(symbol, (obj) =>
            {
            });
            socketClient2.UsdFuturesStreams.SubscribeToOrderBookUpdatesAsync(symbol, 10, (obj) =>
            {
            });
            socketClient2.UsdFuturesStreams.SubscribeToPartialOrderBookUpdatesAsync(symbol, 10, 250, (obj) =>
            {
                Invoke(() =>
                {
                    AskPriceText1.Text = obj.Data.Asks.ElementAt(0).Price.ToString();
                    AskPriceText2.Text = obj.Data.Asks.ElementAt(1).Price.ToString();
                    AskPriceText3.Text = obj.Data.Asks.ElementAt(2).Price.ToString();
                    AskPriceText4.Text = obj.Data.Asks.ElementAt(3).Price.ToString();
                    AskPriceText5.Text = obj.Data.Asks.ElementAt(4).Price.ToString();
                    AskPriceText6.Text = obj.Data.Asks.ElementAt(5).Price.ToString();
                    AskPriceText7.Text = obj.Data.Asks.ElementAt(6).Price.ToString();
                    AskPriceText8.Text = obj.Data.Asks.ElementAt(7).Price.ToString();
                    AskPriceText9.Text = obj.Data.Asks.ElementAt(8).Price.ToString();
                    AskPriceText10.Text = obj.Data.Asks.ElementAt(9).Price.ToString();
                    BidPriceText1.Text = obj.Data.Bids.ElementAt(0).Price.ToString();
                    BidPriceText2.Text = obj.Data.Bids.ElementAt(1).Price.ToString();
                    BidPriceText3.Text = obj.Data.Bids.ElementAt(2).Price.ToString();
                    BidPriceText4.Text = obj.Data.Bids.ElementAt(3).Price.ToString();
                    BidPriceText5.Text = obj.Data.Bids.ElementAt(4).Price.ToString();
                    BidPriceText6.Text = obj.Data.Bids.ElementAt(5).Price.ToString();
                    BidPriceText7.Text = obj.Data.Bids.ElementAt(6).Price.ToString();
                    BidPriceText8.Text = obj.Data.Bids.ElementAt(7).Price.ToString();
                    BidPriceText9.Text = obj.Data.Bids.ElementAt(8).Price.ToString();
                    BidPriceText10.Text = obj.Data.Bids.ElementAt(9).Price.ToString();
                });
            });
        }
    }
}
