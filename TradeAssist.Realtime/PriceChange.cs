namespace TradeAssist.Realtime
{
    public class PriceChange
    {
        public string CurrencyPair { get; set; }
        public decimal VolumeWeightedPrice { get; set; }
        public string Exchange { get; set; }
        public decimal ExchangePrice { get; set; }
    }
}