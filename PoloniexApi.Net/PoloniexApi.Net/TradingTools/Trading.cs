using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Jojatekok.PoloniexAPI.TradingTools
{
    public class Trading : ITrading
    {
        private ApiWebClient ApiWebClient { get; set; }

        internal Trading(ApiWebClient apiWebClient)
        {
            ApiWebClient = apiWebClient;
        }

        private IList<IOrder> GetOpenOrders(CurrencyPair currencyPair)
        {
            var postData = new Dictionary<string, string> {
                { "currencyPair", currencyPair.ToString() }
            };

            var data = PostData<IList<Order>>("returnOpenOrders", postData);
            return (IList<IOrder>)data;
        }

        private async Task<IEnumerable<Trade>> GetTrades(CurrencyPair currencyPair, DateTime startTime, DateTime endTime)
        {
            var postData = new Dictionary<string, string> {
                { "currencyPair", currencyPair.ToString() },
                { "start", Helper.DateTimeToUnixTimeStamp(startTime).ToString() },
                { "end", Helper.DateTimeToUnixTimeStamp(endTime).ToString() }
            };

            var data = await PostData<IEnumerable<Trade>>("returnTradeHistory", postData);
            return data;
        }

        private async Task<ulong> PostOrder(CurrencyPair currencyPair, OrderType type, double pricePerCoin, double amountQuote)
        {
            var postData = new Dictionary<string, string> {
                { "currencyPair", currencyPair.ToString() },
                { "rate", pricePerCoin.ToStringNormalized() },
                { "amount", amountQuote.ToStringNormalized() }
            };

            var data = await PostData<JObject>(type.ToStringNormalized(), postData);
            return data.Value<ulong>("orderNumber");
        }

        private async Task<bool> DeleteOrder(CurrencyPair currencyPair, ulong orderId)
        {
            var postData = new Dictionary<string, string> {
                { "currencyPair", currencyPair.ToString() },
                { "orderNumber", orderId.ToString() }
            };

            var data = await PostData<JObject>("cancelOrder", postData);
            return data.Value<byte>("success") == 1;
        }

        public Task<IList<IOrder>> GetOpenOrdersAsync(CurrencyPair currencyPair)
        {
            return Task.Factory.StartNew(() => GetOpenOrders(currencyPair));
        }

        public Task<IEnumerable<Trade>> GetTradesAsync(CurrencyPair currencyPair, DateTime startTime, DateTime endTime)
        {
            return GetTrades(currencyPair, startTime, endTime);
        }

        public Task<IEnumerable<Trade>> GetTradesAsync(CurrencyPair currencyPair)
        {
            return GetTrades(currencyPair, Helper.DateTimeUnixEpochStart, DateTime.MaxValue);
        }

        public Task<ulong> PostOrderAsync(CurrencyPair currencyPair, OrderType type, double pricePerCoin, double amountQuote)
        {
            return PostOrder(currencyPair, type, pricePerCoin, amountQuote);
        }

        public Task<bool> DeleteOrderAsync(CurrencyPair currencyPair, ulong orderId)
        {
            return DeleteOrder(currencyPair, orderId);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Task<T> PostData<T>(string command, Dictionary<string, string> postData)
        {
            return ApiWebClient.PostData<T>(command, postData);
        }
    }
}
