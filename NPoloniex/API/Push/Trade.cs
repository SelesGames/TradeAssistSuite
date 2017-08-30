namespace NPoloniex.API.Push
{
    public class Trade
    {
        public CurrencyPair CurrencyPair { get; set; }
        public string TradeId { get; set; }
        public TradeType TradeType { get; set; }            // is it a buy or sell?
        public string Price { get; set; }
        public string Quantity { get; set; }
        public long UnixEpochTime { get; set; }
    }

    public enum TradeType
    {
        Buy = 0,   // may have to flip this - currently best guess is that buy is 0 and sell is 1
        Sell = 1
    }
}