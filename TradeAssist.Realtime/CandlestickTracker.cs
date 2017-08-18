using NPoloniex.API.Http;
using NPoloniex.API.Push;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static TradeAssist.Realtime.Constants;

namespace TradeAssist.Realtime
{
    enum CandlestickSize
    {
        _1Minute        = 0,    // 1 minute
        _3Minutes       = 1,    // 3 minutes
        _5Minutes       = 2,    // 5 minutes
        _15Minutes      = 3,    // 15 minutes
        _30Minutes      = 4,    // 30 minutes
        _1Hour          = 5,    // 1 hour
        _2Hours         = 6,    // 2 hours
        _4Hours         = 7,    // 4 hours
        _12Hours        = 8,    // 12 hours
        _1Day           = 9,    // 1 day
        _3Days          = 10,   // 3 days
        _1Week          = 11,   // 1 week
        _1Month         = 12,   // 1 month
        _3Months        = 13    // 3 months
    }

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
        public decimal LowPrice { get; set; }
        public decimal HighPrice { get; set; }
        public DateTime First { get; set; } = DateTime.MaxValue;
        public DateTime Last { get; set; } = DateTime.MinValue;
        public decimal Volume { get; set; }
    }

    /// <summary>
    /// The class that will be emitted via events
    /// </summary>
    public class Candle
    {
        public string CurrencyPair { get; set; }
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

        void InitTimer()
        {
            timer = new System.Threading.Timer(OnTimerTick, null, 0, (int)timerPeriod);
        }

        void OnTimerTick(object _)
        {
            _5minuteTracker += timerPeriod;
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
            }
        }

        void EmitCandles(CandlestickSize candlestickSize)
        {
            var candles = book
                .GetCandles(candlestickSize)
                .Select(o => createCandle(o.Key, o.Value))
                .ToList();

            OnCandlesticksAction?.OnCandlesticks(candles);
            
            Candle createCandle(string key, CandlestickState state)
            {
                return new Candle
                {
                    CurrencyPair = key,
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
                            localLow = 0m,
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