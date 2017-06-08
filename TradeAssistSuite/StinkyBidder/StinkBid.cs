using NPoloniex.API;

namespace TradeAssistSuite
{
    /// <summary>
    /// Represents a "stink bid", a low bid which won't eat into the user's BTC balance as an open order
    /// </summary>
    public class StinkBid
    {
        public UserId UserId { get; set; }
        public CurrencyPair CurrencyPair { get; set; }
        public decimal TriggerPrice { get; set; }
        public decimal BuyPrice { get; set; }
        public decimal Amount { get; set; }
        public StinkBidOrderStatus OrderStatus { get; set; }
    }

    public enum StinkBidOrderStatus
    {
        None,
        Queued,
        Submitted,
        Accepted
    }
}