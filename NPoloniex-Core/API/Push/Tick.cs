namespace NPoloniex.API.Push
{
    public class Tick
    {
        public CurrencyPair CurrencyPair { get; set; }
        public decimal Last { get; set; }
        public decimal LowestAsk { get; set; }
        public decimal HighestBid { get; set; }
        public decimal PercentChange { get; set; }
        public decimal BaseVolume { get; set; }
        public decimal QuoteVolume { get; set; }
        public bool IsFrozen { get; set; }
        public decimal High24Hour { get; set; }
        public decimal Low24Hour { get; set; }
    }
}