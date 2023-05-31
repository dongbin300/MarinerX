using Albedo.Enums;
using Albedo.Extensions;
using Albedo.Mappers;
using Albedo.Models;
using Albedo.Utils;
using Albedo.Views;

using Binance.Net.Clients;

using Bithumb.Net.Clients;
using Bithumb.Net.Enums;

using System.Windows.Controls;
using System.Windows.Media.Animation;

using Upbit.Net.Clients;

namespace Albedo.Managers
{
    public class TickerMan
    {
        #region Binance
        public static void UpdateBinanceSpotTicker(BinanceSocketClient client, MenuControl menu)
        {
            client.SpotApi.ExchangeData.SubscribeToAllTickerUpdatesAsync((obj) =>
            {
                if (Common.CurrentSelectedPairMarket.PairMarket == PairMarket.Favorites) // 즐겨찾기
                {
                    foreach (var item in obj.Data)
                    {
                        var quoteAsset = BinanceSymbolMapper.GetPairQuoteAsset(item.Symbol);
                        var pairId = $"Binance_Spot_{item.Symbol}";
                        if (SettingsMan.FavoritesList.Contains(pairId))
                        {
                            DispatcherService.Invoke(() =>
                            {
                                menu.viewModel.UpdatePairInfo(new Pair(
                               PairMarket.Binance,
                               PairMarketType.Spot,
                               quoteAsset,
                               item.Symbol, item.LastPrice, item.PriceChangePercent));
                            });
                        }
                    }
                    Common.ArrangePairs();
                }
                else if (Common.CurrentSelectedPairMarket.PairMarket == PairMarket.Binance && Common.CurrentSelectedPairMarketType.PairMarketType == PairMarketType.Spot) // 바이낸스 현물
                {
                    foreach (var item in obj.Data)
                    {
                        var quoteAsset = BinanceSymbolMapper.GetPairQuoteAsset(item.Symbol);
                        if (quoteAsset == Common.CurrentSelectedPairQuoteAsset.PairQuoteAsset)
                        {
                            DispatcherService.Invoke(() =>
                            {
                                menu.viewModel.UpdatePairInfo(new Pair(
                                PairMarket.Binance,
                                PairMarketType.Spot,
                                quoteAsset,
                                item.Symbol, item.LastPrice, item.PriceChangePercent));
                            });
                        }
                    }
                    Common.ArrangePairs();
                }
            });
        }

        public static void UpdateBinanceFuturesTicker(BinanceSocketClient client, MenuControl menu)
        {
            client.UsdFuturesStreams.SubscribeToAllTickerUpdatesAsync((obj) =>
            {
                if (Common.CurrentSelectedPairMarket.PairMarket == PairMarket.Favorites) // 즐겨찾기
                {
                    foreach (var item in obj.Data)
                    {
                        var quoteAsset = BinanceSymbolMapper.GetPairQuoteAsset(item.Symbol);
                        var pairId = $"Binance_Futures_{item.Symbol}";
                        if (SettingsMan.FavoritesList.Contains(pairId))
                        {
                            DispatcherService.Invoke(() =>
                            {
                                menu.viewModel.UpdatePairInfo(new Pair(
                                    PairMarket.Binance,
                                    PairMarketType.Futures,
                                    quoteAsset,
                                    item.Symbol, item.LastPrice, item.PriceChangePercent));
                            });
                        }
                    }
                    Common.ArrangePairs();
                }
                else if (Common.CurrentSelectedPairMarket.PairMarket == PairMarket.Binance && Common.CurrentSelectedPairMarketType.PairMarketType == PairMarketType.Futures) // 바이낸스 선물
                {
                    foreach (var item in obj.Data)
                    {
                        var quoteAsset = BinanceSymbolMapper.GetPairQuoteAsset(item.Symbol);
                        if (quoteAsset == Common.CurrentSelectedPairQuoteAsset.PairQuoteAsset)
                        {
                            DispatcherService.Invoke(() =>
                            {
                                menu.viewModel.UpdatePairInfo(new Pair(
                               PairMarket.Binance,
                               PairMarketType.Futures,
                               quoteAsset,
                               item.Symbol, item.LastPrice, item.PriceChangePercent));
                            });
                        }
                    }
                    Common.ArrangePairs();
                }
            });
        }

        public static void UpdateBinanceCoinFuturesTicker(BinanceSocketClient client, MenuControl menu)
        {
            client.CoinFuturesStreams.SubscribeToAllTickerUpdatesAsync((obj) =>
            {
                if (Common.CurrentSelectedPairMarket.PairMarket == PairMarket.Favorites) // 즐겨찾기
                {
                    foreach (var item in obj.Data)
                    {
                        var quoteAsset = BinanceSymbolMapper.GetPairQuoteAsset(item.Symbol);
                        var pairId = $"Binance_CoinFutures_{item.Symbol}";
                        if (SettingsMan.FavoritesList.Contains(pairId))
                        {
                            DispatcherService.Invoke(() =>
                            {
                                menu.viewModel.UpdatePairInfo(new Pair(
                                    PairMarket.Binance,
                                    PairMarketType.CoinFutures,
                                    PairQuoteAsset.USDT,
                                    item.Symbol, item.LastPrice, item.PriceChangePercent));
                            });
                        }
                    }
                    Common.ArrangePairs();
                }
                else if (Common.CurrentSelectedPairMarket.PairMarket == PairMarket.Binance && Common.CurrentSelectedPairMarketType.PairMarketType == PairMarketType.CoinFutures) // 바이낸스 코인선물
                {
                    foreach (var item in obj.Data)
                    {
                        DispatcherService.Invoke(() =>
                        {
                            menu.viewModel.UpdatePairInfo(new Pair(
                            PairMarket.Binance,
                            PairMarketType.CoinFutures,
                            PairQuoteAsset.USDT,
                            item.Symbol, item.LastPrice, item.PriceChangePercent));
                        });
                    }
                    Common.ArrangePairs();
                }
            });
        }
        #endregion

        #region Upbit
        public static void UpdateUpbitSpotTicker(UpbitClient client, MenuControl menu)
        {
            if (Common.CurrentSelectedPairMarket.PairMarket == PairMarket.Favorites) // 즐겨찾기
            {
                var symbols = UpbitSymbolMapper.Symbols;
                var tickerResult = client.QuotationTickers.GetTickersAsync(symbols);
                tickerResult.Wait();
                foreach (var coin in tickerResult.Result)
                {
                    var quoteAsset = UpbitSymbolMapper.GetPairQuoteAsset(coin.market);
                    var pairId = $"Upbit_Spot_{coin.market}";
                    if (SettingsMan.FavoritesList.Contains(pairId))
                    {
                        DispatcherService.Invoke(() =>
                        {
                            menu.viewModel.UpdatePairInfo(new Pair(
                                    PairMarket.Upbit,
                                    PairMarketType.Spot,
                                    quoteAsset,
                                    coin.market, coin.trade_price, coin.signed_change_rate * 100));
                        });
                    }
                }
                Common.ArrangePairs();
            }
            else if (Common.CurrentSelectedPairMarket.PairMarket == PairMarket.Upbit) // 업비트 현물
            {
                var symbols = Common.CurrentSelectedPairQuoteAsset.PairQuoteAsset switch
                {
                    PairQuoteAsset.KRW => UpbitSymbolMapper.KrwSymbols,
                    PairQuoteAsset.BTC => UpbitSymbolMapper.BtcSymbols,
                    PairQuoteAsset.USDT => UpbitSymbolMapper.UsdtSymbols,
                    _ => UpbitSymbolMapper.Symbols,
                };
                var tickerResult = client.QuotationTickers.GetTickersAsync(symbols);
                tickerResult.Wait();
                foreach (var coin in tickerResult.Result)
                {
                    DispatcherService.Invoke(() =>
                    {
                        menu.viewModel.UpdatePairInfo(new Pair(
                       PairMarket.Upbit,
                       PairMarketType.Spot,
                       Common.CurrentSelectedPairQuoteAsset.PairQuoteAsset,
                       coin.market, coin.trade_price, coin.signed_change_rate * 100));
                    });
                }
                Common.ArrangePairs();
            }
        }
        #endregion

        #region Bithumb
        public static void InitLoadBithumbSpotTicker(BithumbClient client, MenuControl menu)
        {
            if (Common.CurrentSelectedPairMarket.PairMarket == PairMarket.Favorites) // 즐겨찾기
            {
                var tickers = client.Public.GetAllTickersAsync(BithumbPaymentCurrency.KRW);
                tickers.Wait();
                foreach (var coin in tickers.Result.data?.coins ?? default!)
                {
                    var pairId = $"Bithumb_Spot_{coin.currency}_KRW";
                    if (SettingsMan.FavoritesList.Contains(pairId))
                    {
                        DispatcherService.Invoke(() =>
                        {
                            menu.viewModel.UpdatePairInfo(new Pair(
                            PairMarket.Bithumb,
                            PairMarketType.Spot,
                            PairQuoteAsset.KRW,
                            $"{coin.currency}_KRW", coin.closing_price, coin.fluctate_rate_24H));
                        });
                    }
                }
                tickers = client.Public.GetAllTickersAsync(BithumbPaymentCurrency.BTC);
                tickers.Wait();
                foreach (var coin in tickers.Result.data?.coins ?? default!)
                {
                    var pairId = $"Bithumb_Spot_{coin.currency}_BTC";
                    if (SettingsMan.FavoritesList.Contains(pairId))
                    {
                        DispatcherService.Invoke(() =>
                        {
                            menu.viewModel.UpdatePairInfo(new Pair(
                            PairMarket.Bithumb,
                            PairMarketType.Spot,
                            PairQuoteAsset.BTC,
                            $"{coin.currency}_BTC", coin.closing_price, coin.fluctate_rate_24H));
                        });
                    }
                }
            }
            else if (Common.CurrentSelectedPairMarket.PairMarket == PairMarket.Bithumb && Common.CurrentSelectedPairMarketType.PairMarketType == PairMarketType.Spot) // 빗썸 현물
            {
                var paymentCurrency = Common.CurrentSelectedPairQuoteAsset.PairQuoteAsset.ToBithumbPaymentCurrency();
                var tickers = client.Public.GetAllTickersAsync(paymentCurrency);
                tickers.Wait();
                foreach (var coin in tickers.Result.data?.coins ?? default!)
                {
                    DispatcherService.Invoke(() =>
                    {
                        menu.viewModel.UpdatePairInfo(new Pair(
                        PairMarket.Bithumb,
                        PairMarketType.Spot,
                        Common.CurrentSelectedPairQuoteAsset.PairQuoteAsset,
                        $"{coin.currency}_{paymentCurrency}", coin.closing_price, coin.fluctate_rate_24H));
                    });
                }
            }
        }

        public static void UpdateBithumbSpotTicker(BithumbSocketClient client, MenuControl menu)
        {
            client.Streams.SubscribeToTickerAsync(BithumbSymbolMapper.Symbols, BithumbSocketTickInterval.OneDay, (obj) =>
            {
                var data = obj.content;

                if (Common.CurrentSelectedPairMarket.PairMarket == PairMarket.Favorites) // 즐겨찾기
                {
                    var pairId = $"Bithumb_Spot_{data.symbol}";
                    var quoteAsset = BithumbSymbolMapper.GetPairQuoteAsset(data.symbol);
                    DispatcherService.Invoke(() =>
                    {
                        if (SettingsMan.FavoritesList.Contains(pairId))
                        {
                            menu.viewModel.UpdatePairInfo(new Pair(
                                PairMarket.Bithumb,
                                PairMarketType.Spot,
                                quoteAsset,
                                data.symbol, data.closePrice, data.chgRate));
                        }
                    });
                    Common.ArrangePairs();
                }
                else if (Common.CurrentSelectedPairMarket.PairMarket == PairMarket.Bithumb) // 빗썸 현물
                {
                    var quoteAsset = BithumbSymbolMapper.GetPairQuoteAsset(data.symbol);
                    if (quoteAsset == Common.CurrentSelectedPairQuoteAsset.PairQuoteAsset)
                    {
                        DispatcherService.Invoke(() =>
                        {
                            menu.viewModel.UpdatePairInfo(new Pair(
                          PairMarket.Bithumb,
                          PairMarketType.Spot,
                          quoteAsset,
                          data.symbol, data.closePrice, data.chgRate));
                        });
                    }
                    Common.ArrangePairs();
                }
            });
        }
        #endregion
    }
}
