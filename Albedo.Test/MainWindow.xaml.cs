using Binance.Net.Clients;

using System.Windows;

namespace Albedo.Test
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        BinanceSocketClient binanceSocketClient = new();

        public MainWindow()
        {
            InitializeComponent();

            binanceSocketClient.UsdFuturesStreams.SubscribeToAllTickerUpdatesAsync((obj) =>
            {
                DispatcherService.Invoke(() =>
                {
                    var data = obj.Data;
                    foreach (var item in data)
                    {
                        ViewModel.UpdatePair(item.Symbol, item.LastPrice.ToString());
                    }
                });
            });
        }
    }
}
