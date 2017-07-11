using System;
using System.Collections.Generic;

namespace NPoloniex.API.Push
{
    /// <summary>
    /// Class to manage subscriptions to ticker updates for a particular currency pair
    /// </summary>
    class TickerSubscriptions
    {
        Dictionary<CurrencyPair, List<OnCurrencyTickerUpdated>> tickerSubscribers =
            new Dictionary<CurrencyPair, List<OnCurrencyTickerUpdated>>();

        List<OnCurrencyTickerUpdated> rawSubscribers = new List<OnCurrencyTickerUpdated>();

        public void NotifySubscribers(CurrencyPair currencyPair, Tick tick)
        {
            List<OnCurrencyTickerUpdated> subscribers;

            if (tickerSubscribers.TryGetValue(currencyPair, out subscribers))
            {
                foreach (var subscriber in subscribers)
                    subscriber(tick);
            }
        }

        public void NotifySubscribers(Tick tick)
        {
            foreach (var subscriber in rawSubscribers)
                subscriber(tick);
        }

        public IDisposable Subscribe(CurrencyPair currencyPair, OnCurrencyTickerUpdated onTickerUpdated)
        {
            List<OnCurrencyTickerUpdated> subscriberList;

            if (tickerSubscribers.ContainsKey(currencyPair))
            {
                subscriberList = tickerSubscribers[currencyPair];      
            }
            else
            {
                subscriberList = new List<OnCurrencyTickerUpdated>();
                tickerSubscribers.Add(currencyPair, subscriberList);
            }

            subscriberList.Add(onTickerUpdated);

            return new DisposalHelper(() => subscriberList.Remove(onTickerUpdated));
        }

        public IDisposable Subscribe(OnCurrencyTickerUpdated onTickerUpdated)
        {
            rawSubscribers.Add(onTickerUpdated);

            return new DisposalHelper(() => rawSubscribers.Remove(onTickerUpdated));
        }

        class DisposalHelper : IDisposable
        {
            readonly Action onDispose;

            public DisposalHelper(Action onDispose)
            {
                this.onDispose = onDispose;
            }

            public void Dispose()
            {
                onDispose();
            }
        }
    }
}