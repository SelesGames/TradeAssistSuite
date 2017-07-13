using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static NPoloniex.API.Http.NonceCalculator;

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
        public Task<List<MarketChartData>> GetChartData(CurrencyPair currencyPair, CandlestickSize candlestickSize, DateTime startTime, DateTime endTime)
        {
            string[] parameters =  {
                "currencyPair=" + currencyPair,
                "start=" + DateTimeToUnixTimeStamp(startTime),
                "end=" + DateTimeToUnixTimeStamp(endTime),
                "period=" + ((int)candlestickSize).ToString()
            };

            return ApiHttpClient.GetData<List<MarketChartData>>("returnChartData", ApiUrlHttpsRelativeTrading, parameters);
        }
    }
}