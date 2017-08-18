using NPoloniex.API.Http;
using NPoloniex.API.Push;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using static TradeAssist.Realtime.Constants;

namespace TradeAssist.Realtime
{
    /*internal static class Constants
    {
        public static readonly string Poloniex = "poloniex";
        public static readonly string Bittrex = "bittrex";

        static readonly CultureInfo culture = new CultureInfo("en-us");

        static readonly char CanonicalSeparator = '-';

        public static readonly char BittrexSeparator = '-';
        public static readonly char PoloniexSeparator = '_';

        public static string Canonical(string input, char separator, bool reverse = false)
        {
            var pairs = input.Split(separator);

            if (pairs.Length != 2)
                return null;

            if (reverse)
            {
                return join(pairs[1], pairs[0]);
            }
            else
            {
                return join(pairs[0], pairs[1]);
            }

            string join(string a, string b)
            {
                return $"{a.ToUpper(culture)}{CanonicalSeparator}{b.ToUpper(culture)}";
            }
        }

        public static string Canonical(string input)
        {
            return
                Canonical(input, BittrexSeparator) ??
                Canonical(input, PoloniexSeparator);
        }

        public static List<string> Exchanges()
        {
            return new List<string> { Bittrex, Poloniex };
        }
    }*/

    internal interface IOnPriceChangeAction
    {
        void OnPriceChange(PriceChange pc);
    }

    class PriceTracker
    {
        Ticker poloniexTicker = Ticker.Current;
        Dictionary<string, PriceInfo> priceInfoLookup = new Dictionary<string, PriceInfo>();

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
                                volume: item.BaseVolume);
                        }
                    };

                await bittrexHub.Initialize();
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
            }
        }

        void OnPriceEvent(string currencyPair, string exchange, decimal price, decimal volume)
        {
            var previousPrice = priceInfoLookup.TryGetValue(currencyPair, out var priceInfo) ?
                priceInfo.Prices.TryGetValue(exchange, out var tuple) ? tuple.price : -1 : -1;

            if (price == previousPrice)
                return;

            priceInfo = priceInfo ?? new PriceInfo();
            priceInfo.AdjustForLatestPrice(exchange, price, volume);
            priceInfoLookup[currencyPair] = priceInfo;

            var priceChange = new PriceChange
            {
                CurrencyPair = currencyPair,
                VolumeWeightedPrice = priceInfo.VolumeWeightedPrice,
                Exchange = exchange,
                ExchangePrice = price,
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
    }
}