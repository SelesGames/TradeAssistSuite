using NPoloniex.API.Http;
using NPoloniex.API.Push;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static TradeAssist.Realtime.Constants;
using System.Collections;
using System.Collections.Concurrent;

namespace TradeAssist.Realtime.Ticker
{
    public class PriceTracker : IEnumerable<PriceInfo>
    {
        internal IDictionary<string, PriceInfo> priceInfoLookup = new Dictionary<string, PriceInfo>();
        internal IDictionary<string, Dictionary<string, ExchangePriceInfo>> exchangeLookup = new Dictionary<string, Dictionary<string, ExchangePriceInfo>>();

        public static PriceTracker Current { get; } = new PriceTracker();
        private PriceTracker() { }

        public IOnPriceChangeAction OnPriceChangeAction { get; set; }

        public async Task Initialize()
        {
            await InitializePoloniex();
            Console.WriteLine("poloniex initialized");

            await InitializeBittrex();
            Console.WriteLine("bittrex initialized");

            async Task InitializeBittrex()
            {
                var bittrexHub = new NBittrex.BittrexHub();
                bittrexHub.OnUpdateSummaryStateTick += (_, tick) =>
                    {
                        if (tick == null || tick.Deltas == null)
                            return;

                        foreach (var item in tick.Deltas)
                        {
                            OnPriceEvent(
                                currencyPair: Canonical(item.MarketName, BittrexSeparator), 
                                exchange: Bittrex, 
                                price: item.Last, 
                                volume: item.BaseVolume,
                                bid: item.Bid,
                                ask: item.Ask);
                        }
                    };

                await bittrexHub.Connect();
            }

            async Task InitializePoloniex()
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
                        volume: val.BaseVolume,
                        bid: val.HighestBid,
                        ask: val.LowestAsk);
                }

                var poloniexTicker = NPoloniex.API.Push.Ticker.Current;
                poloniexTicker.Tick += (_, tick) =>
                    OnPriceEvent(
                        currencyPair: Canonical(tick.CurrencyPair, PoloniexSeparator),
                        exchange: Poloniex, 
                        price: tick.Last, 
                        volume: tick.BaseVolume,
                        bid: tick.HighestBid,
                        ask: tick.LowestAsk);

                await poloniexTicker.Initialize();

                var client = new PoloniexWebSocketClient();
                await client.Connect();
                await client.SubscribeToTicker();
            }
        }

        void OnPriceEvent(
            string currencyPair, 
            string exchange, 
            decimal price, 
            decimal volume,
            decimal bid,
            decimal ask)
        {
            /*var previousPrice = priceInfoLookup.TryGetValue(currencyPair, out var priceInfo) ?
                priceInfo.Prices.TryGetValue(exchange, out var tuple) ? tuple.price : -1 : -1;

            if (price == previousPrice)
                return;*/

            priceInfoLookup.TryGetValue(currencyPair, out var priceInfo);

            priceInfo = priceInfo ?? new PriceInfo(currencyPair);
            var exchangePriceInfo = new ExchangePriceInfo(exchange, price, volume, bid, ask);
            priceInfo.AdjustForLatestPrice(exchange, exchangePriceInfo);
            priceInfoLookup[currencyPair] = priceInfo;
            var list = exchangeLookup.TryGetValue(exchange, out var exchangeCurrencyList) ? exchangeCurrencyList : new Dictionary<string, ExchangePriceInfo>();
            list[currencyPair] = exchangePriceInfo;
            exchangeLookup[exchange] = list;

            var priceChange = new PriceChange
            {
                CurrencyPair = currencyPair,
                VolumeWeightedPrice = priceInfo.VolumeWeightedPrice,
                Exchange = exchange,
                Price = price,
                HighestBid = bid,
                LowestAsk = ask
            };

            OnPriceChangeAction?.OnPriceChange(priceChange);
        }

        public PriceInfo GetPriceInfo(string currencyPair, bool requiresCleanup = false)
        {
            if (requiresCleanup)
                currencyPair = Canonical(currencyPair);

            return priceInfoLookup.TryGetValue(currencyPair, out var val) ? val : null;
        }

        public List<string> GetMarkets(string exchange = null)
        {
            exchange = exchange?.ToLowerInvariant();
            if (!Exchanges().Contains(exchange))
                exchange = null;

            var markets = priceInfoLookup
                .SelectMany(a => a.Value.Prices
                    .Select(b => new { exchange = b.Key, currencyPair = a.Key }));

            if (exchange != null)
                markets = markets.Where(o => o.exchange == exchange);

            return markets.Select(o => o.currencyPair).Distinct().OrderBy(o => o).ToList();
        }




        #region IEnumerable interface

        IEnumerable<PriceInfo> GetEnumeratorAdaptor()
        {
            foreach (var priceInfo in priceInfoLookup)
            {
                yield return priceInfo.Value;
            }
        }

        public IEnumerator<PriceInfo> GetEnumerator()
        {
            return GetEnumeratorAdaptor().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumeratorAdaptor().GetEnumerator();
        }

        #endregion IEnumerable interface
    }
}