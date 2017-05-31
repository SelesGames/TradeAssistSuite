using System;
using System.Collections.Generic;

namespace NPoloniex.API.Push
{
    public delegate void OnCurrencyTickerUpdated(Tick tick);

    public class Ticker
    {
        public static Ticker Current = new Ticker();
        private Ticker() { }

        Dictionary<CurrencyPair, Tick> map = new Dictionary<CurrencyPair, Tick>();
        TickerSubscriptions subscriptions = new TickerSubscriptions();

        public Tick this[CurrencyPair key]
        {
            get
            {
                return map[key];
            }
        }

        public Tick GetCurrentPrice(CurrencyPair currencyPair)
        {
            return map[currencyPair];
        }

        public void Update(Tick tick)
        {
            var key = tick.CurrencyPair;

            /*if (map.ContainsKey(key))
                map[key] = tick;
            else
                map.Add(key, tick);*/
            map[key] = tick;

            subscriptions.NotifySubscribers(key, tick);
        }

        public IDisposable Subscribe(CurrencyPair currencyPair, OnCurrencyTickerUpdated onTickerUpdated)
        {
            return subscriptions.Subscribe(currencyPair, onTickerUpdated);
        }
    }
}