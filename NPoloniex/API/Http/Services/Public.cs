using System.Collections.Generic;
using System.Threading.Tasks;

namespace NPoloniex.API.Http
{
    public class Public
    {
        const string ApiUrlHttpsRelativeTrading = "public";

        ApiHttpClient ApiHttpClient { get; set; }

        internal Public(ApiHttpClient apiHttpClient)
        {
            ApiHttpClient = apiHttpClient;
        }

        public Task<Dictionary<string, MarketData>> GetTicker()
        {
            return ApiHttpClient.GetData<Dictionary<string, MarketData>>("returnTicker", ApiUrlHttpsRelativeTrading);
        }
    }
}