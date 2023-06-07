using MarinerX.Bot.Clients;
using MarinerX.Bot.Models;

using Skender.Stock.Indicators;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace MarinerX.Bot.Bots
{
    public class ManagerBot : Bot
    {
        public ManagerBot() : this("", "")
        {

        }

        public ManagerBot(string name) : this(name, "")
        {

        }

        public ManagerBot(string name, string description)
        {
            Name = name;
            Description = description;
        }

        /// <summary>
        /// Get Total Balance(USDT) and Available Balance(USDT) and BNB Balance(BNB)
        /// </summary>
        /// <returns></returns>
        public async Task<(double, double, double)> GetBinanceBalance()
        {
            try
            {
                var result = await BinanceClients.Api.UsdFuturesApi.Account.GetBalancesAsync().ConfigureAwait(false);
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

        public async Task GetBinancePositions()
        {
            try
            {
                var result = await BinanceClients.Api.UsdFuturesApi.Account.GetPositionInformationAsync().ConfigureAwait(false);
                var positions = result.Data.Where(r => r.Quantity != 0);
                Account.Positions = positions.Select(p => new BinancePosition(
                    p.Symbol,
                    p.PositionSide.ToString(),
                    p.UnrealizedPnl,
                    p.EntryPrice,
                    p.MarkPrice,
                    p.Quantity,
                    p.Leverage
                    )).ToList();
            }
            catch (Exception ex)
            {
                Logger.Log(nameof(ManagerBot), MethodBase.GetCurrentMethod()?.Name, ex);
            }
        }

        public async Task<IEnumerable<BinanceRealizedPnlHistory>> GetBinanceTodayRealizedPnlHistory()
        {
            try
            {
                var result = await BinanceClients.Api.UsdFuturesApi.Account.GetIncomeHistoryAsync(null, "REALIZED_PNL", DateTime.UtcNow.Date).ConfigureAwait(false);
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

        public async Task GetAllKlines()
        {
            try
            {
                foreach(var symbol in MonitorSymbols)
                {
                    var result = await BinanceClients.Api.UsdFuturesApi.ExchangeData.GetKlinesAsync(symbol, Common.BaseInterval, null, null, 31).ConfigureAwait(false);
                    var quotes = result.Data.Select(x => new Quote
                    {
                        Date = x.OpenTime,
                        Open = x.OpenPrice,
                        High = x.HighPrice,
                        Low = x.LowPrice,
                        Close = x.ClosePrice,
                        Volume = x.Volume
                    });

                    Account.PairQuotes.Add(new PairQuote(symbol, quotes));
                }

                Account.AddHistory("Get All Klines");
            }
            catch (Exception ex)
            {
                Logger.Log(nameof(ManagerBot), MethodBase.GetCurrentMethod()?.Name, ex);
            }
        }

        public async Task StartBinanceFuturesTicker()
        {
            try
            {
                await BinanceClients.Socket.UsdFuturesStreams.SubscribeToAllTickerUpdatesAsync((obj) =>
                {
                    foreach (var item in obj.Data.Where(item => MonitorSymbols.Contains(item.Symbol)))
                    {
                        var pairQuote = Account.PairQuotes.Find(x => x.Symbol.Equals(item.Symbol));
                        var quote = new Quote
                        {
                            Date = item.OpenTime,
                            Open = item.OpenPrice,
                            High = item.HighPrice,
                            Low = item.LowPrice,
                            Close = item.LastPrice,
                            Volume = item.Volume
                        };

                        pairQuote?.UpdateQuote(quote);
                        pairQuote?.UpdateIndicators();
                    }
                }).ConfigureAwait(false);

                Account.AddHistory("Start Binance Futures Ticker");
            }
            catch (Exception ex)
            {
                Logger.Log(nameof(ManagerBot), MethodBase.GetCurrentMethod()?.Name, ex);
            }
        }

        public async Task StopBinanceFuturesSubscriptions()
        {
            try
            {
                await BinanceClients.Socket.UsdFuturesStreams.UnsubscribeAllAsync().ConfigureAwait(false);

                Account.AddHistory("Stop Binance Futures Ticker");
            }
            catch (Exception ex)
            {
                Logger.Log(nameof(ManagerBot), MethodBase.GetCurrentMethod()?.Name, ex);
            }
        }
    }
}
