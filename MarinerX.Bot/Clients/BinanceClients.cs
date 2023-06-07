using Binance.Net.Clients;
using Binance.Net.Objects;

using System;
using System.IO;
using System.Reflection;

namespace MarinerX.Bot.Clients
{
    public class BinanceClients
    {
        public static BinanceClient Api = default!;
        public static BinanceSocketClient Socket = default!;

        string listenKey = string.Empty;

        public BinanceClients()
        {

        }

        public static void Init()
        {
            try
            {
                var data = File.ReadAllLines(Common.BinanceApiKeyPath);
                Api = new BinanceClient(new BinanceClientOptions()
                {
                    ApiCredentials = new BinanceApiCredentials(data[0], data[1])
                });
                //RefreshUserStream();

                Socket = new BinanceSocketClient(new BinanceSocketClientOptions()
                {
                    ApiCredentials = new BinanceApiCredentials(data[0], data[1])
                });
                // socketClient.UsdFuturesStreams.SubscribeToUserDataUpdatesAsync() 이거 죽어도 데이터 안옴(추후)
            }
            catch (Exception ex)
            {
                Logger.Log(nameof(BinanceClients), MethodBase.GetCurrentMethod()?.Name, ex);
            }
        }

        private void RefreshUserStream()
        {
            var userStream = Api.UsdFuturesApi.Account.StartUserStreamAsync();
            userStream.Wait();
            listenKey = userStream.Result.Data;

            KeepAliveUserStream();
        }

        private void KeepAliveUserStream()
        {
            var userStream = Api.UsdFuturesApi.Account.KeepAliveUserStreamAsync(listenKey);
            userStream.Wait();
        }
    }
}
