using System;

namespace TradeAssist.Realtime.Candlestick
{
    /// <summary>
    /// The class that will be emitted via events
    /// </summary>
    public class Candle
    {
        public string CurrencyPair { get; set; }
        public double CandleSize { get; set; }
        public string CandleSizeText { get; set; }
        public decimal Open { get; set; }
        public decimal Close { get; set; }
        public decimal High { get; set; }
        public decimal Low { get; set; }
        public decimal Volume { get; set; }
        public DateTime Began { get; set; }
        public DateTime Ended { get; set; }
    }
}