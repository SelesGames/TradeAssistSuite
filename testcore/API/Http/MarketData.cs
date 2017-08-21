using System;
using System.Collections.Generic;

namespace NBittrex.API.Http
{
    public class BittrexResult<T>
    {
        public string Success { get; set; }
        public string Message { get; set; }
        public T Result { get; set; }
    }

    public class GetMarketsResult
    {
        public string Success { get; set; }
        public string Message { get; set; }
        public List<MarketData> Result { get; set; }
    }

    public class MarketData
    {
        public string MarketCurrency { get; set; }
        public string BaseCurrency { get; set; }
        public string MarketCurrencyLong { get; set; }
        public string BaseCurrencyLong { get; set; }
        public string MarketName { get; set; }
        public bool IsActive { get; set; }
        public DateTime Created { get; set; }
    }
}