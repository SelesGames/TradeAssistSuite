using System;
using static TradeAssist.Realtime.Candlestick.CandlestickSize;

namespace TradeAssist.Realtime.Candlestick
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
        _1Month         = 12,   // 1 month (30 days)
        _3Months        = 13    // 3 months (91 days)
    }

    static class CandlestickSizeExtensions
    {
        static readonly double _1minute = TimeSpan.FromMinutes(1).TotalMilliseconds;
        static readonly double _3minutes = TimeSpan.FromMinutes(3).TotalMilliseconds;
        static readonly double _5minutes = TimeSpan.FromMinutes(5).TotalMilliseconds;
        static readonly double _15minutes = TimeSpan.FromMinutes(15).TotalMilliseconds;
        static readonly double _30minutes = TimeSpan.FromMinutes(30).TotalMilliseconds;
        static readonly double _1hour = TimeSpan.FromHours(1).TotalMilliseconds;
        static readonly double _2hours = TimeSpan.FromHours(2).TotalMilliseconds;
        static readonly double _4hours = TimeSpan.FromHours(4).TotalMilliseconds;
        static readonly double _12hours = TimeSpan.FromHours(12).TotalMilliseconds;
        static readonly double _1day = TimeSpan.FromDays(1).TotalMilliseconds;
        static readonly double _3days = TimeSpan.FromDays(3).TotalMilliseconds;
        static readonly double _1week = TimeSpan.FromDays(7).TotalMilliseconds;
        static readonly double _1month = TimeSpan.FromDays(30).TotalMilliseconds;
        static readonly double _3months = TimeSpan.FromDays(91).TotalMilliseconds;

        public static double GetSizeInMilliseconds(this CandlestickSize size)
        {
            switch (size)
            {
                case _1Minute:      return _1minute;
                case _3Minutes:     return _3minutes;
                case _5Minutes:     return _5minutes;
                case _15Minutes:    return _15minutes;
                case _30Minutes:    return _30minutes;
                case _1Hour:        return _1hour;
                case _2Hours:       return _2hours;
                case _4Hours:       return _4hours;
                case _12Hours:      return _12hours;
                case _1Day:         return _1day;
                case _3Days:        return _3days;
                case _1Week:        return _1week;
                case _1Month:       return _1month;
                case _3Months:      return _3months;

                default: throw new Exception($"unrecognized CandlestickSize: {size}");
            }
        }
    }
}