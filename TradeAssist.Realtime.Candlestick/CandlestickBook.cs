using System;
using System.Collections.Generic;
using System.Linq;

namespace TradeAssist.Realtime.Candlestick
{
    class CandlestickBook
    {
        public static CandlestickBook Current { get; } = new CandlestickBook();

        private CandlestickBook()
        {
            CandlestickStates = Enum.GetNames(typeof(CandlestickSize)).Select(_ =>
                new Dictionary<string, CandlestickState>()).ToList();
        }

        public List<Dictionary<string, CandlestickState>> CandlestickStates { get; }

        public CandlestickState GetCandle(CandlestickSize size, string currencyPair)
        {
            return GetCandles(size).TryGetValue(currencyPair, out var candle) ? candle : null;
        }

        public IReadOnlyDictionary<string, CandlestickState> GetCandles(CandlestickSize size)
        {
            return CandlestickStates[(int)size];
        }
    }

    class CandlestickState
    {
        public decimal FirstPrice { get; set; }
        public decimal LastPrice { get; set; }
        public decimal LowPrice { get; set; } = decimal.MaxValue;
        public decimal HighPrice { get; set; }
        public DateTime First { get; set; } = DateTime.MaxValue;
        public DateTime Last { get; set; } = DateTime.MinValue;
        public decimal Volume { get; set; }
        public DateTime Began { get; set; }
        public DateTime Ended { get; set; }

        private CandlestickState() { }

        public static CandlestickState New()
        {
            var state = new CandlestickState();
            state.Reset();
            return state;
        }

        public void Reset()
        {
            FirstPrice = 0m;
            LastPrice = 0m;
            LowPrice = decimal.MaxValue;
            HighPrice = 0m;
            First = DateTime.MaxValue;
            Last = DateTime.MinValue;
            Volume = 0m;
            Began = DateTime.UtcNow;
        }
    }
}