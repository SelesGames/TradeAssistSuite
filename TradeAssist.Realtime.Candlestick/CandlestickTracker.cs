using NPoloniex.API.Http;
using NPoloniex.API.Push;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static TradeAssist.Realtime.Constants;
using static TradeAssist.Realtime.Candlestick.CandlestickSize;

namespace TradeAssist.Realtime.Candlestick
{
    class CandlestickBook
    {
        public CandlestickBook()
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

        public void Reset()
        {
            FirstPrice = 0m;
            LastPrice = 0m;
            LowPrice = decimal.MaxValue;
            HighPrice = 0m;
            First = DateTime.MaxValue;
            Last = DateTime.MinValue;
            Volume = 0m;
        }
    }

    /// <summary>
    /// The class that will be emitted via events
    /// </summary>
    public class Candle
    {
        public string CurrencyPair { get; set; }
        public double CandleSize { get; set; }
        public decimal Open { get; set; }
        public decimal Close { get; set; }
        public decimal High { get; set; }
        public decimal Low { get; set; }
        public decimal Volume { get; set; }
    }

    internal interface IOnCandlesticksAction
    {
        void OnCandlesticks(List<Candle> pc);
    }

    class TimerState
    {
        readonly double candleLengthInMilliseconds;
        double tracker = 0d;

        public CandlestickSize CandlestickSize { get; }

        public TimerState(CandlestickSize size)
        {
            this.candleLengthInMilliseconds = size.GetSizeInMilliseconds();
            CandlestickSize = size;
        }

        public bool Increment(double timerPeriod)
        {
            tracker += timerPeriod;
            if (tracker > candleLengthInMilliseconds)
            {
                tracker = 0d;
                return true;
            }
            return false;
        }
    }

    class CandlestickTracker
    {
        public static CandlestickTracker Current { get; } = new CandlestickTracker();
        private CandlestickTracker() { }

        public IOnCandlesticksAction OnCandlesticksAction { get; set; }

        CandlestickBook book = new CandlestickBook();
        Timer timer;
        const double timerPeriod = 50d; // interval in ms.  50 = 20 times per second

        readonly double _5minutes = TimeSpan.FromMinutes(5).TotalMilliseconds;
        double _5minuteTracker = 0d;

        readonly double _1minute = TimeSpan.FromMinutes(1).TotalMilliseconds;
        double _1minuteTracker = 0d;

        List<TimerState> timerStates = Enum.GetValues(typeof(CandlestickSize))
            .Cast<CandlestickSize>()
            .Select(o => new TimerState(o))
            .ToList();

        void InitTimer()
        {
            timer = new System.Threading.Timer(OnTimerTick, null, 0, (int)timerPeriod);
        }

        void OnTimerTick(object _)
        {
            foreach (var timerState in timerStates)
            {
                if (timerState.Increment(timerPeriod))
                {
                    EmitCandles(timerState.CandlestickSize);
                }
            }
            /*_5minuteTracker += timerPeriod;
            if (_5minuteTracker > _5minutes)
            {
                EmitCandles(CandlestickSize._5Minutes);
                _5minuteTracker = 0d;
            }

            _1minuteTracker += timerPeriod;
            if (_1minuteTracker > _1minute)
            {
                EmitCandles(CandlestickSize._1Minute);
                _1minuteTracker = 0d;
            }*/
        }

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

            OnCandlesticksAction?.OnCandlesticks(candles);
            
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
        }

        public async Task Initialize()
        {
            //await InitializePoloniex();
            //Console.WriteLine("poloniex initialized");

            await InitializeBittrex();
            Console.WriteLine("bittrex initialized");

            InitTimer();

            async Task InitializeBittrex()
            {
                var bittrexHub = new NBittrex.BittrexHub();
                bittrexHub.OnUpdateExchangeStateTick += (_, tick) =>
                    {
                        if (tick == null || tick.Fills == null || !tick.Fills.Any())
                            return;

                        decimal
                            localHigh = 0m,
                            localLow = decimal.MaxValue,
                            lastPrice = 0m,
                            firstPrice = 0m,
                            aggregateVolume = 0m;

                        DateTime first = DateTime.MaxValue, last = DateTime.MinValue;

                        foreach (var item in tick.Fills)
                        {
                            var rate = item.Rate;

                            // Update local low and local high price
                            if (rate < localLow)
                                localLow = rate;

                            if (rate > localHigh)
                                localHigh = rate;


                            // Update the first and last prices
                            var timeStamp = item.TimeStamp;

                            if (timeStamp < first)
                            {
                                first = timeStamp;
                                firstPrice = rate;
                            }

                            if (timeStamp > last)
                            {
                                last = timeStamp;
                                lastPrice = rate;
                            }

                            // Update the aggregate volume
                            aggregateVolume += item.Quantity;
                        }

                        OnFillEvent(
                            currencyPair: Canonical(tick.Marketname, BittrexSeparator),
                            exchange: Bittrex,
                            localLow: localLow,
                            localHigh: localHigh,
                            firstPrice: firstPrice,
                            lastPrice: lastPrice,
                            first: first,
                            last: last,
                            volume: aggregateVolume);
                    };

                await bittrexHub.Initialize();

                var httpClient = new NBittrex.API.Http.ApiHttpClient();
                var markets = await httpClient.Public.GetMarkets();
                var marketNames = markets.Result.Select(o => o.MarketName);
                // FOR DEBUG ONLY
                marketNames = marketNames.Where(o => o == "BTC-ANS");

                foreach (var marketName in marketNames)
                {
                    await bittrexHub.SubscribeToTicker(marketName);
                }
            }

            /*async Task InitializePoloniex()
            {
                // initialize current price
                var restClient = new ApiHttpClient();
                var marketData = await restClient.Public.GetTicker();
                foreach (var market in marketData)
                {
                    var val = market.Value;
                    OnPriceEvent(
                        currencyPair: Canonical(market.Key, PoloniexSeparator),
                        exchange: Poloniex, 
                        price: val.Last, 
                        volume: val.BaseVolume);
                }

                poloniexTicker.Tick += (_, tick) =>
                    OnPriceEvent(
                        currencyPair: Canonical(tick.CurrencyPair, PoloniexSeparator),
                        exchange: Poloniex, 
                        price: tick.Last, 
                        volume: tick.BaseVolume);

                await poloniexTicker.Initialize();

                var client = new PoloniexWebSocketClient();
                await client.Connect();
                await client.SubscribeToTicker();
            }*/
        }

        void OnFillEvent(
            string currencyPair,
            string exchange,
            decimal localLow,
            decimal localHigh,
            decimal firstPrice,
            decimal lastPrice,
            DateTime first,
            DateTime last,
            decimal volume)
        {
            foreach (var candleStickLookup in book.CandlestickStates)
            {
                if (!candleStickLookup.TryGetValue(currencyPair, out var candle))
                {
                    candle = new CandlestickState();
                    candleStickLookup[currencyPair] = candle;
                }

                // update candle low and high prices
                if (localLow < candle.LowPrice)
                    candle.LowPrice = localLow;

                if (localHigh > candle.HighPrice)
                    candle.HighPrice = localHigh;

                // Update candle first and last prices
                if (first < candle.First)
                {
                    candle.First = first;
                    candle.FirstPrice = firstPrice;
                }

                if (last > candle.Last)
                {
                    candle.Last = last;
                    candle.LastPrice = lastPrice;
                }

                // Update the candle volume
                candle.Volume += volume;
            }
        }
    }
}