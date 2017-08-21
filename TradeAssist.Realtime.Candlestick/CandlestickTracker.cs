using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static TradeAssist.Realtime.Constants;

namespace TradeAssist.Realtime.Candlestick
{
    internal interface IOnCandlesticksAction
    {
        void OnCandlesticks(List<Candle> pc);
    }

    class CandlestickTracker
    {
        public static CandlestickTracker Current { get; } = new CandlestickTracker();
        private CandlestickTracker() { }

        readonly CandlestickBook book = CandlestickBook.Current;

        public async Task Initialize()
        {
            //await InitializePoloniex();
            //Console.WriteLine("poloniex initialized");

            await InitializeBittrex();
            Console.WriteLine("bittrex initialized");

            async Task InitializeBittrex()
            {
                var bittrexHub = new NBittrex.BittrexHub();
                bittrexHub.OnUpdateExchangeStateTick += (_, tick) =>
                    {
                        if (tick == null || tick.Fills == null || !tick.Fills.Any())
                            return;

                        decimal
                            localHigh = 0m,
                            localLow = decimal.MaxValue,
                            lastPrice = 0m,
                            firstPrice = 0m,
                            aggregateVolume = 0m;

                        DateTime first = DateTime.MaxValue, last = DateTime.MinValue;

                        foreach (var item in tick.Fills)
                        {
                            Console.WriteLine($"Received trade event: Bittrex {item.OrderType} {tick.Marketname}, {item.Quantity} at {item.Rate}");

                            var rate = item.Rate;

                            // Update local low and local high price
                            if (rate < localLow)
                                localLow = rate;

                            if (rate > localHigh)
                                localHigh = rate;


                            // Update the first and last prices
                            var timeStamp = item.TimeStamp;

                            if (timeStamp < first)
                            {
                                first = timeStamp;
                                firstPrice = rate;
                            }

                            if (timeStamp > last)
                            {
                                last = timeStamp;
                                lastPrice = rate;
                            }

                            // Update the aggregate volume
                            aggregateVolume += item.Quantity;
                        }

                        OnFillEvent(
                            currencyPair: Canonical(tick.Marketname, BittrexSeparator),
                            exchange: Bittrex,
                            localLow: localLow,
                            localHigh: localHigh,
                            firstPrice: firstPrice,
                            lastPrice: lastPrice,
                            first: first,
                            last: last,
                            volume: aggregateVolume);
                    };

                await bittrexHub.Initialize();

                var httpClient = new NBittrex.API.Http.ApiHttpClient();
                var markets = await httpClient.Public.GetMarkets();
                var marketNames = markets.Result.Select(o => o.MarketName);
                // FOR DEBUG ONLY
                marketNames = marketNames.Where(o => o == "BTC-NEO");

                foreach (var marketName in marketNames)
                {
                    var isSubscribed = await bittrexHub.SubscribeToTicker(marketName);
                    if (isSubscribed)
                        Console.WriteLine($"Subscribed to {marketName}");
                    else
                        Console.WriteLine($"FAILED to subscribe to {marketName}");
                }
            }

            /*async Task InitializePoloniex()
            {
                // initialize current price
                var restClient = new ApiHttpClient();
                var marketData = await restClient.Public.GetTicker();
                foreach (var market in marketData)
                {
                    var val = market.Value;
                    OnPriceEvent(
                        currencyPair: Canonical(market.Key, PoloniexSeparator),
                        exchange: Poloniex, 
                        price: val.Last, 
                        volume: val.BaseVolume);
                }

                poloniexTicker.Tick += (_, tick) =>
                    OnPriceEvent(
                        currencyPair: Canonical(tick.CurrencyPair, PoloniexSeparator),
                        exchange: Poloniex, 
                        price: tick.Last, 
                        volume: tick.BaseVolume);

                await poloniexTicker.Initialize();

                var client = new PoloniexWebSocketClient();
                await client.Connect();
                await client.SubscribeToTicker();
            }*/
        }

        void OnFillEvent(
            string currencyPair,
            string exchange,
            decimal localLow,
            decimal localHigh,
            decimal firstPrice,
            decimal lastPrice,
            DateTime first,
            DateTime last,
            decimal volume)
        {
            foreach (var candleStickLookup in book.CandlestickStates)
            {
                if (!candleStickLookup.TryGetValue(currencyPair, out var candle))
                {
                    candle = CandlestickState.New();
                    candleStickLookup[currencyPair] = candle;
                }

                // update candle low and high prices
                if (localLow < candle.LowPrice)
                    candle.LowPrice = localLow;

                if (localHigh > candle.HighPrice)
                    candle.HighPrice = localHigh;

                // Update candle first and last prices
                if (first < candle.First)
                {
                    candle.First = first;
                    candle.FirstPrice = firstPrice;
                }

                if (last > candle.Last)
                {
                    candle.Last = last;
                    candle.LastPrice = lastPrice;
                }

                // Update the candle volume
                candle.Volume += volume;
            }
        }
    }
}