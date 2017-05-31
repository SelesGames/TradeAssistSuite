using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace NPoloniex.API.Http
{
    public class Trading
    {
        const string ApiUrlHttpsRelativeTrading = "tradingApi";

        ApiHttpClient ApiHttpClient { get; set; }

        internal Trading(ApiHttpClient apiHttpClient)
        {
            ApiHttpClient = apiHttpClient;
        }

        /* private IList<IOrder> GetOpenOrders(CurrencyPair currencyPair)
         {
             var postData = new Dictionary<string, string> {
                 { "currencyPair", currencyPair.ToString() }
             };

             var data = PostData<IList<Order>>("returnOpenOrders", postData);
             return (IList<IOrder>)data;
         }*/

        /*public Task<IEnumerable<Trade>> GetTradesAsync(CurrencyPair currencyPair)
        {
            return GetTrades(currencyPair, Helper.DateTimeUnixEpochStart, DateTime.MaxValue);
        }*/

        public Task<IEnumerable<Trade>> GetTrades(CurrencyPair currencyPair, DateTime startTime, DateTime endTime)
        {
            var postData = new Dictionary<string, string> {
                { "currencyPair", currencyPair.ToString() },
                { "start", NonceCalculator.DateTimeToUnixTimeStamp(startTime).ToString() },
                { "end", NonceCalculator.DateTimeToUnixTimeStamp(endTime).ToString() }
            };

            return ApiHttpClient.PostData<IEnumerable<Trade>>("returnTradeHistory", ApiUrlHttpsRelativeTrading, postData);
        }

        async Task<ulong> PostOrder(CurrencyPair currencyPair, OrderType orderType, decimal rate, decimal amount)
        {
            var postData = new Dictionary<string, string> {
                { "currencyPair", currencyPair.ToString() },
                { "rate", rate.ToString() },
                { "amount", amount.ToString() }
            };

            string command;
            if (orderType == OrderType.Buy)
                command = "buy";
            else if (orderType == OrderType.Sell)
                command = "sell";
            else
                throw new Exception("Unrecognized Ordertype in PostOrder");

            var response = await ApiHttpClient.PostData<JObject>(command, ApiUrlHttpsRelativeTrading, postData);
            return response.Value<ulong>("orderNumber");
        }

        public Task<ulong> Buy(CurrencyPair currencyPair, decimal rate, decimal amount)
        {
            return PostOrder(currencyPair, OrderType.Buy, rate, amount);
        }

        public Task<ulong> Sell(CurrencyPair currencyPair, decimal rate, decimal amount)
        {
            return PostOrder(currencyPair, OrderType.Sell, rate, amount);
        }

        public async Task<bool> CancelOrder(CurrencyPair currencyPair, ulong orderNumber)
        {
            var postData = new Dictionary<string, string> {
                { "currencyPair", currencyPair.ToString() },
                { "orderNumber", orderNumber.ToString() }
            };

            var data = await ApiHttpClient.PostData<JObject>("cancelOrder", ApiUrlHttpsRelativeTrading, postData);
            return data.Value<byte>("success") == 1;
        }

        public Task<JObject> MoveOrder(ulong orderNumber, decimal rate, decimal? amount = null)
        {
            var postData = new Dictionary<string, string> {
                { "orderNumber", orderNumber.ToString() },
                { "rate", rate.ToString() }
            };

            if (amount.HasValue)
                postData.Add("amount", amount.ToString());

            return ApiHttpClient.PostData<JObject>("moveOrder", ApiUrlHttpsRelativeTrading, postData);
        }
    }
}