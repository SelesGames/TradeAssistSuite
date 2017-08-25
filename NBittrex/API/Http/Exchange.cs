using Shared.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace NBittrex.API.Http
{
    public class Exchange
    {
        const string ApiCallTemplate = "https://bittrex.com/api/{0}/{1}";
        const string ApiVersion = "v1.1";
        const string ApiCallGetMarkets = "public/getmarkets";
        const string ApiCallGetTicker = "public/getticker";
        const string ApiCallGetOrderBook = "public/getorderbook";
        const string ApiCallGetMarketHistory = "public/getmarkethistory";
        const string ApiCallGetMarketSummary = "public/getmarketsummary";

        const string ApiCallGetBalances = "account/getbalances";
        const string ApiCallGetBalance = "account/getbalance";
        const string ApiCallGetOrderHistory = "account/getorderhistory";

        const string ApiCallBuyLimit = "market/buylimit";
        const string ApiCallSellLimit = "market/selllimit";
        const string ApiCallGetOpenOrders = "market/getopenorders";
        const string ApiCallCancel = "market/cancel";

        string apiKey;
        string secret;
        bool simulate;
        ApiCall apiCall;

        public Exchange(ExchangeContext context)
        {
            this.apiKey = context.ApiKey;
            this.secret = context.Secret;
            this.simulate = context.Simulate;
            this.apiCall = new ApiCall(this.simulate);
        }

        public Task<AccountBalance> GetBalance(string market)
        {
            return this.Call<AccountBalance>(ApiCallGetBalance, ("currency", market));
        }

        public Task<GetBalancesResponse> GetBalances()
        {
            return this.Call<GetBalancesResponse>(ApiCallGetBalances);
        }

        public Task<OrderResponse> PlaceBuyOrder(string market, decimal quantity, decimal price)
        {
            return this.Call<OrderResponse>(ApiCallBuyLimit, ("market", GetMarketName(market)), ("quantity", quantity.ToString()), ("rate", price.ToString()));
        }

        public Task<OrderResponse> PlaceSellOrder(string market, decimal quantity, decimal price)
        {
            return this.Call<OrderResponse>(ApiCallSellLimit, ("market", GetMarketName(market)), ("quantity", quantity.ToString()), ("rate", price.ToString()));
        }

        public decimal CalculateMinimumOrderQuantity(string market, decimal price)
        {
            var minimumQuantity = Math.Round(0.00050000M / price, 1) + 0.1M;
            return minimumQuantity;
        }

        public Task<List<MarketData>> GetMarkets()
        {
            return this.Call<List<MarketData>>(ApiCallGetMarkets);
        }

        public Task<dynamic> GetTicker(string market)
        {
            return this.Call<dynamic>(ApiCallGetTicker, ("market", GetMarketName(market)));
        }

        public Task<GetOpenOrdersResponse> GetOpenOrders(string market)
        {
            return this.Call<GetOpenOrdersResponse>(ApiCallGetOpenOrders, ("market", GetMarketName(market)));
        }

        public Task CancelOrder(string uuid)
        {
            return this.Call<dynamic>(ApiCallCancel, ("uuid", uuid));
        }

        public async Task<GetOrderBookResponse> GetOrderBook(string market, OrderBookType type, int depth = 20)
        {
            if (type == OrderBookType.Both)
            {
                return await this.Call<GetOrderBookResponse>(ApiCallGetOrderBook,
                    ("market", GetMarketName(market)),
                    ("type", type.ToString().ToLower()),
                    ("depth", depth.ToString()));
            }
            else
            {
                var results = await this.Call<List<OrderEntry>>(ApiCallGetOrderBook,
                    ("market", GetMarketName(market)),
                    ("type", type.ToString().ToLower()),
                    ("depth", depth.ToString()));

                if (type == OrderBookType.Buy)
                {
                    return new GetOrderBookResponse { Buy = results };
                }
                else
                {
                    return new GetOrderBookResponse { Sell = results };
                }
            }
        }

        public Task<GetMarketHistoryResponse> GetMarketHistory(string market, int count = 20)
        {
            return this.Call<GetMarketHistoryResponse>(ApiCallGetMarketHistory,
                ("market", GetMarketName(market)),
                ("count", count.ToString()));
        }

        public async Task<GetMarketSummaryResponse> GetMarketSummary(string market)
        {
            var response = await this.Call<GetMarketSummaryResponse[]>(ApiCallGetMarketSummary,
                ("market", GetMarketName(market)));
            return response.Single();
        }

        public Task<GetOrderHistoryResponse> GetOrderHistory(string market, int count = 20)
        {
            return this.Call<GetOrderHistoryResponse>(ApiCallGetOrderHistory,
                ("market", GetMarketName(market)),
                ("count", count.ToString()));
        }

        private static string HashHmac(string message, string secret)
        {
            Encoding encoding = Encoding.UTF8;
            using (HMACSHA512 hmac = new HMACSHA512(encoding.GetBytes(secret)))
            {
                var msg = encoding.GetBytes(message);
                var hash = hmac.ComputeHash(msg);
                return BitConverter.ToString(hash).ToLower().Replace("-", string.Empty);
            }
        }

        string GetMarketName(string market)
        {
            if (!market.Contains("-"))
                return $"BTC-{market}";
            else
                return market.ToUpperInvariant();
        }

        Task<T> Call<T>(string method, params (string, object)[] parameters)
        {
            var uriBuilder = new StringBuilder(string.Format(ApiCallTemplate, ApiVersion, method));
            var uriParametersBuilder = new UriParametersBuilder();

            // append parameters
            uriParametersBuilder.AddParameters(parameters);

            // if non-public api, append api key and nonce parameters
            if (!method.StartsWith("public"))
            {
                var nonce = DateTime.UtcNow.Ticks;
                uriParametersBuilder.AddParameters(("apikey", apiKey), ("nonce", nonce));
            }

            uriBuilder.Append(uriParametersBuilder.ToString());
            var uri = uriBuilder.ToString();

            if (method.StartsWith("public"))
            {
                return this.apiCall.CallWithJsonResponse<T>(uri, false);
            }
            // if non-public api, sign the request via Hmac hash, insert as a header
            else
            {
                var sign = HashHmac(uri, secret);

                return this.apiCall.CallWithJsonResponse<T>(uri,
                    DoesCallHaveEffects(),
                    Tuple.Create("apisign", sign));
            }

            bool DoesCallHaveEffects()
            {
                return !method.StartsWith("market/get") && !method.StartsWith("account/get");
            }
        }
    }
}