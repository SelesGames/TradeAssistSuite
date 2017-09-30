using NPoloniex.API.Push;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GeneralTester
{
    class PoloniexOrderBook : IOnOrderAction
    {
        public string Market { get; }
        List<OrderEntry> Bids { get; } = new List<OrderEntry>();
        List<OrderEntry> Asks { get; } = new List<OrderEntry>();

        readonly OrderComparer bidsComparer = new OrderComparer(OrderType.Bid);
        readonly OrderComparer asksComparer = new OrderComparer(OrderType.Ask);

        private PoloniexOrderBook(string market)
        {
            Market = market;
        }

        public static async Task<PoloniexOrderBook> Create(PoloniexWebSocketClient2 client, string market)
        {
            // pull order book for market to initialize
            var pob = new PoloniexOrderBook(market);
            await client.Connect();
            await client.SubscribeToTrades(market);

            // refresh order book via api here, then immediately attach an onorderaction
            client.OnOrderAction = pob;

            return pob;
        }

        public void OnOrderEvent(Order order)
        {
            if (!decimal.TryParse(order.Rate, out var rate))
            {
                // inform error
                return;
            }
            if (!decimal.TryParse(order.Amount, out var amount))
            {
                // inform error
                return;
            }

            var orderEntry = new OrderEntry(rate, amount);

            if (order.OrderType == OrderType.Bid)
                InsertSorted(Bids, bidsComparer);
            else if (order.OrderType == OrderType.Ask)
                InsertSorted(Asks, asksComparer);

            void InsertSorted(List<OrderEntry> list, OrderComparer orderComparer)
            {
                var binarySearchIndex = list.BinarySearch(orderEntry, orderComparer);
                if (binarySearchIndex < 0)
                {
                    // the case where the item does not already exist in the list
                    if (amount > 0)
                    {
                        list.Insert(~binarySearchIndex, orderEntry);
                    }
                }
                else
                {
                    // the case where the item is already present.  We need to either remove,
                    // if amount = 0, or update if amount > 0
                    if (amount == 0)
                    {
                        list.RemoveAt(binarySearchIndex);
                    }
                    else
                    {
                        list[binarySearchIndex].Amount = amount;
                    }
                }
            }
        }

        class OrderComparer : IComparer<OrderEntry>
        {
            readonly OrderType orderType;

            public OrderComparer(OrderType orderType)
            {
                this.orderType = orderType;
            }

            public int Compare(OrderEntry x, OrderEntry y)
            {
                if (orderType == OrderType.Ask)
                {
                    return ~x.Rate.CompareTo(y.Rate);
                }
                else if (orderType == OrderType.Bid)
                {
                    return x.Rate.CompareTo(y.Rate);
                }
                else throw new Exception("Can't compare orders of two different types");
            }
        }
    }

    class OrderEntry
    {
        public decimal Rate { get; set; }
        public decimal Amount { get; set; }

        public OrderEntry(decimal rate, decimal amount)
        {
            Rate = rate;
            Amount = amount;
        }
    }
}