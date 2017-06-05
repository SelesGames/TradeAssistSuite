using NPoloniex.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeAssistSuite
{
    public enum StinkBidOrderStatus
    {
        None,
        Queued,
        Submitted,
        Accepted
    }

    /// <summary>
    /// Represents a "stink bid", a low bid which won't eat into the user's BTC balance as an open order
    /// </summary>
    public class StinkBid
    {
        public CurrencyPair CurrencyPair { get; set; }
        public decimal BuyPrice { get; set; }
        public decimal Amount { get; set; }
        public StinkBidOrderStatus OrderStatus { get; set; }
    }
}
