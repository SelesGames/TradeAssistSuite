using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using NBittrex.API.Http;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace testAzureFunction
{
    public static class GetBalance
    {
        [FunctionName("GetBalances")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "GetBalances/{id}")]
            HttpRequestMessage req, 
            
            string id, 
          
            TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

            var user = await GetUser(id);

            if (user == null)
            {
                return req.CreateErrorResponse(HttpStatusCode.NotFound, new Exception("no such user with id exists"));
            }

            var bittrexClient = new Exchange(
                new ExchangeContext
                {
                    ApiKey = user.BittrexApiKey,
                    Secret = user.Secret,
                    Simulate = false,
                });

            var balances = await bittrexClient.GetBalances();

            var summary = new TradeGuardSummary();

            foreach (var balance in balances)
            {
                var openOrders = await bittrexClient.GetOpenOrders(balance.Currency);
                decimal totalQuantity = 0;
                foreach (var openOrder in openOrders.Where(o => IsValidStopLimitOrder(o)))
                {
                    totalQuantity += openOrder.Quantity;
                }

                var breakdown = new HoldingBreakdown
                {
                    Exchange = "bittrex",
                    ExchangeUnits = balance.Balance,
                    ExchangeProtected = totalQuantity,
                    ExchangePercentProtected = (double)(totalQuantity / balance.Balance),
                    PercentOnExchange = 1d,
                };

                var holding = new Holding
                {
                    CurrencyPair = balance.Currency,
                    TotalUnits = breakdown.ExchangeUnits,
                    TotalProtected = breakdown.ExchangeProtected,
                    TotalPercentProtected = breakdown.ExchangePercentProtected,
                };

                holding.Breakdown.Add(breakdown);

                summary.Holdings.Add(holding);
            }

            //var deserialized = JsonConvert.SerializeObject(summary);

            var response = req.CreateResponse(HttpStatusCode.OK, summary, "application/json");
            return response;
        }

        static bool IsValidStopLimitOrder(OpenOrder o)
        {
            return o.OrderType == OpenOrderType.Limit_Sell;
            //&& o.Price < currentPrice // need current price!
        }

        static async Task<dynamic> GetUser(string userId)
        {
            await Task.Delay(0);
            return new
            {
                BittrexApiKey = "",
                Secret = "",
            };
        }
    }
}