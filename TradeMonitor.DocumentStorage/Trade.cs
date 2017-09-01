namespace TradeMonitor.DocumentStorage
{
    public class Trade
    {
        public string Id { get; set; }
        public string Exchange { get; set; }
        public string Market { get; set; }
        public TradeType Type { get; set; }            // is it a buy or sell?
        public decimal Price { get; set; }
        public decimal Quantity { get; set; }
        public long Time { get; set; }
    }

    public enum TradeType
    {
        Buy = 0,
        Sell = 1
    }
}