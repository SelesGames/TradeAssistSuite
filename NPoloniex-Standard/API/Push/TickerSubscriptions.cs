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

        public void NotifySubscribers(CurrencyPair currencyPair, Tick tick)
        {
            List<OnCurrencyTickerUpdated> subscribers;

            if (tickerSubscribers.TryGetValue(currencyPair, out subscribers))
            {
                foreach (var subscriber in subscribers)
                    subscriber(tick);
            }
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

            return new SubscriptionDisposalHelper(subscriberList, onTickerUpdated);
        }

        class SubscriptionDisposalHelper : IDisposable
        {
            private List<OnCurrencyTickerUpdated> list;
            private OnCurrencyTickerUpdated subscriptionAction;

            public SubscriptionDisposalHelper(List<OnCurrencyTickerUpdated> list, OnCurrencyTickerUpdated subscriptionAction)
            {
                this.list = list;
                this.subscriptionAction = subscriptionAction;
            }

            public void Dispose()
            {
                list.Remove(subscriptionAction);
            }
        }
    }
}