using Binance.Net.Clients;
using Binance.Net.Objects;
using Binance.Net.Objects.Models.Futures;

using MarinerX.Bot.Models;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MarinerX.Bot.Managers
{
    public class BinanceManager
    {
        BinanceClient client = default!;
        BinanceSocketClient socketClient = default!;

        string listenKey = string.Empty;

        public List<string> MonitorSymbols = new()
        {
            "AAVEUSDT",
            "ADAUSDT",
            "ALGOUSDT",
            "ALPHAUSDT",
            "ATOMUSDT",
            "AVAXUSDT",
            "AXSUSDT",
            "BALUSDT",
            "BANDUSDT",
            "BATUSDT",
            "BCHUSDT",
            "BELUSDT",
            "BLZUSDT",
            "BNBUSDT",
            "BTCUSDT",
            "COMPUSDT",
            "CRVUSDT",
            "CTKUSDT",
            "CVCUSDT",
            "DASHUSDT",
            "DOGEUSDT",
            "DOTUSDT",
            "EGLDUSDT",
            "ENJUSDT",
            "EOSUSDT",
            "ETCUSDT",
            "ETHUSDT",
            "FILUSDT",
            "FLMUSDT",
            "FTMUSDT",
            "GRTUSDT",
            "ICXUSDT",
            "IOSTUSDT",
            "IOTAUSDT",
            "KAVAUSDT",
            "KNCUSDT",
            "KSMUSDT",
            "LRCUSDT",
            "LTCUSDT",
            "MATICUSDT",
            "MKRUSDT",
            "NEARUSDT",
            "NEOUSDT",
            "OCEANUSDT",
            "OMGUSDT",
            "ONTUSDT",
            "QTUMUSDT",
            "RENUSDT",
            "RLCUSDT",
            "RSRUSDT",
            "RUNEUSDT",
            "SKLUSDT",
            "SNXUSDT",
            "SOLUSDT",
            "STORJUSDT",
            "SUSHIUSDT",
            "SXPUSDT",
            "THETAUSDT",
            "TOMOUSDT",
            "TRBUSDT",
            "TRXUSDT",
            "UNFIUSDT",
            "UNIUSDT",
            "VETUSDT",
            "WAVESUSDT",
            "XLMUSDT",
            "XMRUSDT",
            "XRPUSDT",
            "XTZUSDT",
            "YFIUSDT",
            "ZECUSDT",
            "ZENUSDT",
            "ZILUSDT",
            "ZRXUSDT"
        };

        public BinanceManager()
        {
            var data = File.ReadAllLines(Common.BinanceApiKeyPath);
            client = new BinanceClient(new BinanceClientOptions()
            {
                ApiCredentials = new BinanceApiCredentials(data[0], data[1])
            });
            //RefreshUserStream();

            socketClient = new BinanceSocketClient(new BinanceSocketClientOptions()
            {
                ApiCredentials = new BinanceApiCredentials(data[0], data[1])
            });

            // socketClient.UsdFuturesStreams.SubscribeToUserDataUpdatesAsync() 이거 죽어도 데이터 안옴(추후)
        }

        private void RefreshUserStream()
        {
            var userStream = client.UsdFuturesApi.Account.StartUserStreamAsync();
            userStream.Wait();
            listenKey = userStream.Result.Data;

            KeepAliveUserStream();
        }

        private void KeepAliveUserStream()
        {
            var userStream = client.UsdFuturesApi.Account.KeepAliveUserStreamAsync(listenKey);
            userStream.Wait();
        }

        /// <summary>
        /// Get Total Balance(USDT) and Available Balance(USDT) and BNB Balance(BNB)
        /// </summary>
        /// <returns></returns>
        public async Task<(double, double, double)> GetBinanceBalance()
        {
            try
            {
                var result = await client.UsdFuturesApi.Account.GetBalancesAsync().ConfigureAwait(false);
                var balance = result.Data;
                var usdtBalance = balance.First(b => b.Asset.Equals("USDT"));
                var usdt = usdtBalance.WalletBalance + usdtBalance.CrossUnrealizedPnl;
                var availableUsdt = (double)Math.Round(usdtBalance.AvailableBalance, 3);
                var total = (double)Math.Round(usdt, 3);
                var bnb = (double)Math.Round(balance.First(b => b.Asset.Equals("BNB")).WalletBalance, 4);

                return (total, availableUsdt, bnb);
            }
            catch
            {
                return (Common.NullDoubleValue, Common.NullDoubleValue, Common.NullDoubleValue);
            }
        }

        public async Task<IEnumerable<BinancePositionDetailsUsdt>> GetBinancePositions()
        {
            try
            {
                var result = await client.UsdFuturesApi.Account.GetPositionInformationAsync().ConfigureAwait(false);
                return result.Data.Where(r=>r.Quantity != 0);
            }
            catch
            {
                return default!;
            }
        }

        public async Task<IEnumerable<BinanceRealizedPnlHistory>> GetBinanceTodayRealizedPnlHistory()
        {
            try
            {
                var result = await client.UsdFuturesApi.Account.GetIncomeHistoryAsync(null, "REALIZED_PNL", DateTime.UtcNow.Date).ConfigureAwait(false);
                var data = result.Data;
                return data.Select(d => new BinanceRealizedPnlHistory(
                    d.Timestamp,
                    d.Symbol ?? string.Empty,
                    (double)d.Income
                    ));
            }
            catch
            {
                return default!;
            }
        }
    }
}
