using NPoloniex.API.Http;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NPoloniex.API.Push
{
    public delegate void OnCurrencyTickerUpdated(Tick tick);

    public class Ticker
    {
        public static Ticker Current = new Ticker();
        private Ticker() { }

        Dictionary<CurrencyPair, Tick> map = new Dictionary<CurrencyPair, Tick>();
        TickerSubscriptions subscriptions = new TickerSubscriptions();
        BootupStatus status = BootupStatus.None;

        public event EventHandler<Tick> Tick;

        public async Task Initialize()
        {
            if (status == BootupStatus.Initializing || status == BootupStatus.Initialized)
                return;

            status = BootupStatus.Initializing;

            var client = new ApiHttpClient();
            var dict = await client.Public.GetTicker();
            foreach (var item in dict)
            {
                var val = item.Value;
                var tick = new Tick
                {
                    CurrencyPair = item.Key,
                    Last = val.Last,
                    LowestAsk = val.LowestAsk,
                    HighestBid = val.HighestBid,
                    PercentChange = val.PercentChange,
                    BaseVolume = val.BaseVolume,
                    QuoteVolume = val.Quotevolume,
                    IsFrozen = val.IsFrozen,
                };
                map[tick.CurrencyPair] = tick;
            }

            status = BootupStatus.Initialized;
        }

        public Tick this[CurrencyPair key] => GetCurrentPrice(key);

        public Tick GetCurrentPrice(CurrencyPair currencyPair)
        {
            if (status == BootupStatus.Initialized)
            {
                if (map.TryGetValue(currencyPair, out var tick))
                    return tick;
                else
                    throw new KeyNotFoundException($"Could not find {currencyPair.ToString()} in the Ticker");
            }
            else
                throw new UnauthorizedAccessException("Must call and await \"Markets.Initialize() before accessing CurrencyPairs");
        }

        public void Update(Tick tick)
        {
            var key = tick.CurrencyPair;
            map[key] = tick;
            subscriptions.NotifySubscribers(key, tick);
            subscriptions.NotifySubscribers(tick);
            Tick?.Invoke(this, tick);
        }

        public IDisposable Subscribe(CurrencyPair currencyPair, OnCurrencyTickerUpdated onTickerUpdated)
        {
            return subscriptions.Subscribe(currencyPair, onTickerUpdated);
        }

        public IDisposable Subscribe(OnCurrencyTickerUpdated onTickerUpdated)
        {
            return subscriptions.Subscribe(onTickerUpdated);
        }
    }
}