namespace TradeAssist.Realtime.Ticker
{
    public class ExchangePriceInfo
    {
        public ExchangePriceInfo(
            string exchange, 
            decimal price, 
            decimal volume, 
            decimal highestBid, 
            decimal lowestAsk)
        {
            Exchange = exchange;
            Price = price;
            Volume = volume;
            HighestBid = highestBid;
            LowestAsk = lowestAsk;
        }

        public string Exchange { get; }
        public decimal Price { get; }
        public decimal Volume { get; }
        public decimal HighestBid { get; }
        public decimal LowestAsk { get; }
    }
}