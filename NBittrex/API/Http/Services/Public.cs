using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NBittrex.API.Http
{
    public class Public
    {
        const string ApiUrlHttpsRelativeTrading = "public";

        ApiHttpClient ApiHttpClient { get; }

        internal Public(ApiHttpClient apiHttpClient)
        {
            ApiHttpClient = apiHttpClient;
        }

        public Task<BittrexResult<List<MarketData>>> GetMarkets()
        {
            return ApiHttpClient.GetData<BittrexResult<List<MarketData>>>("getMarkets", ApiUrlHttpsRelativeTrading);
        }
    }
}