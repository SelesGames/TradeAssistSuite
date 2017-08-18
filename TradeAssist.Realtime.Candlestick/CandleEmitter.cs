using System;
using System.Collections.Generic;

namespace TradeAssist.Realtime.Candlestick
{
    /// <summary>
    /// Serves as a broker between the timing mechanism and taking snapshots of candles
    /// </summary>
    class CandleEmitter : IEmitCandles
    {
        readonly IOnCandlesticksAction onCandlesticksAction;
        readonly CandlestickBook book = CandlestickBook.Current;

        public CandleEmitter(IOnCandlesticksAction onCandlesticksAction)
        {
            this.onCandlesticksAction = onCandlesticksAction;
        }

        public void EmitCandles(IEnumerable<CandlestickSize> candleSizes)
        {
            //var ended = DateTime.UtcNow;

            var candles = new List<Candle>();

            foreach (var candlestickSize in candleSizes)
            {
                var states = book.GetCandles(candlestickSize);

                foreach (var state in states)
                {
                    state.Value.Ended = DateTime.UtcNow;
                    var candle = createCandle(state.Key, state.Value, candlestickSize);
                    state.Value.Reset();
                    candles.Add(candle);
                }
            }

            onCandlesticksAction?.OnCandlesticks(candles);

            Candle createCandle(string key, CandlestickState state, CandlestickSize candleSize)
            {
                return new Candle
                {
                    CurrencyPair = key,
                    CandleSize = candleSize.GetSizeInMilliseconds(),
                    CandleSizeText = candleSize.GetDisplayLabel(),
                    Open = state.FirstPrice,
                    Close = state.LastPrice,
                    High = state.HighPrice,
                    Low = state.LowPrice,
                    Volume = state.Volume,
                    Began = state.Began,
                    Ended = state.Ended,
                };
            }
        }
        /*
        void EmitCandles(CandlestickSize candlestickSize)
        {
            var states = book.GetCandles(candlestickSize);

            var candles = new List<Candle>(states.Count);
            foreach (var state in states)
            {
                var candle = createCandle(state.Key, state.Value);
                state.Value.Reset();
                candles.Add(candle);
            }

            onCandlesticksAction?.OnCandlesticks(candles);

            Candle createCandle(string key, CandlestickState state)
            {
                return new Candle
                {
                    CurrencyPair = key,
                    CandleSize = candlestickSize.GetSizeInMilliseconds(),
                    Open = state.FirstPrice,
                    Close = state.LastPrice,
                    High = state.HighPrice,
                    Low = state.LowPrice,
                    Volume = state.Volume,
                };
            }
        }*/
    }
}