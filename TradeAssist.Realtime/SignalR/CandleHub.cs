using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Owin;
using System.Linq;
using static TradeAssist.Realtime.Constants;

namespace TradeAssist.Realtime
{
    /// <summary>
    /// Simple SignalR hub that provides Pub/Sub for ticker pairs, as well as retrieval of last price
    /// </summary>
    [HubName("candle")]
    public class CandleHub : Hub
    {
        public void Subscribe(string currencyPair)
        {
            var groupName = Canonical(currencyPair);
            if (groupName == null)
                return;

            Groups.Add(Context.ConnectionId, "oneMinute");
        }

        public void Unsubscribe(string currencyPair)
        {
            var groupName = Canonical(currencyPair);
            if (groupName == null)
                return;

            Groups.Remove(Context.ConnectionId, "oneMinute");
        }
        /*
        public object Last(string currencyPair)
        {
            var canonical = Canonical(currencyPair);

            var last = PriceTracker.Current.GetPriceInfo(canonical);
            if (last != null)
            {
                return new
                {
                    currencyPair = canonical,
                    volumeWeightedPrice = last.VolumeWeightedPrice,
                    prices = last.Prices.Select(o =>
                    new
                    {
                        exchange = o.Key,
                        price = o.Value.price,
                        volume = o.Value.volume
                    }).ToList()
                };
            }
            else
            {
                return new { error = "not found" };
            }
        }

        public object Markets(string exchange = null)
        {
            return PriceTracker.Current.GetMarkets(exchange);
        }*/
    }
}