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

        public Task<ApiCallResponse<List<MarketData>>> GetMarkets()
        {
            return ApiHttpClient.GetData<ApiCallResponse<List<MarketData>>>("getMarkets", ApiUrlHttpsRelativeTrading);
        }
    }
}