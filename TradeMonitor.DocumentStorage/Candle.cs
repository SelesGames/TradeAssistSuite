namespace TradeMonitor.DocumentStorage
{
    public class Candle
    {
        public string Id { get; set; }
        public string Exchange { get; set; }
        public string Market { get; set; }
        public double CandleSize { get; set; }
        public decimal Open { get; set; }
        public decimal Close { get; set; }
        public decimal High { get; set; }
        public decimal Low { get; set; }
        public decimal Volume { get; set; }
        public long Began { get; set; }
    }
}