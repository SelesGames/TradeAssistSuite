using Akka.Actor;
using NPoloniex.API;
using NPoloniex.API.Push;
using System;
using System.Collections.Generic;

namespace TradeAssistSuite.StinkyBidder.Actors
{
    /// <summary>
    /// Class that monitors price changes, and manages a list of all StinkBids for a particular 
    /// CurrencyPair.  It also dequeues stinkbids from the list once a target price is met,
    /// and sends those off to another queue that will actually execute them.
    /// </summary>
    public class Monitor : ReceiveActor
    {
        readonly CurrencyPair currencyPair;
        readonly List<StinkBid> stinkBids = new List<StinkBid>();
        readonly static StinkBidComparer stinkBidComparer = new StinkBidComparer();

        public Monitor(CurrencyPair currencyPair)
        {
            this.currencyPair = currencyPair;

            Receive<StinkBid>(OnStinkBidReceived);
            Receive<Tick>(OnPriceChange);
        }

        protected override void PreStart()
        {
            base.PreStart();
            Console.WriteLine($"Starting {Self.Path.ToString()}");
        }

        bool OnStinkBidReceived(StinkBid stinkBid)
        {
            if (stinkBid == null || stinkBid.CurrencyPair != currencyPair)
                return false;

            var binarySearchIndex = stinkBids.BinarySearch(stinkBid, stinkBidComparer);
            if (binarySearchIndex < 0)
            {
                stinkBids.Insert(~binarySearchIndex, stinkBid);
            }
            else
            {
                stinkBids.Insert(binarySearchIndex, stinkBid);
            }

            return true;
        }

        bool OnPriceChange(Tick tick)
        {
            if (tick.CurrencyPair != currencyPair)
                return false;

            var currentPrice = Math.Min(tick.Last, tick.LowestAsk);

            int index;
            for (index = 0; index < stinkBids.Count; index++)
            {
                var stinkBid = stinkBids[index];

                // stop iterating the moment you run out of trigger prices that are less than current price
                if (currentPrice > stinkBid.TriggerPrice)
                    break;

                Queue(stinkBid);
            }

            stinkBids.RemoveRange(0, index);

            return true;
        }

        void Queue(StinkBid stinkBid)
        {
            // send message to database here to update the stinkbid status
            var receiver = MyActorSystem.Current.GetExecutor();
            receiver.Tell(stinkBid);
        }

        class StinkBidComparer : IComparer<StinkBid>
        {
            public int Compare(StinkBid x, StinkBid y)
            {
                return x.TriggerPrice.CompareTo(y.TriggerPrice);
            }
        }
    }

    public class User
    {
        public UserId UserId { get; set; }
        public string PublicKey { get; set; }
        public string PrivateKey { get; set; }
    }
}