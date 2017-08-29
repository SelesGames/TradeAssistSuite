namespace TradeAssist.Realtime.Ticker
{
    public class PriceChange
    {
        public string CurrencyPair { get; set; }
        public string Exchange { get; set; }
        public decimal Price { get; set; }
        public decimal VolumeWeightedPrice { get; set; }
        public decimal HighestBid { get; set; }
        public decimal LowestAsk { get; set; }
    }
}