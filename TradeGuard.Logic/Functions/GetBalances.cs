using NBittrex.API.Http;
using System.Linq;
using System.Threading.Tasks;

namespace TradeGuard.Logic
{
    public static partial class Functions
    {
        public static async Task<(TradeGuardSummary summary, dynamic error)> GetBalances(string userId, string bittrexApiKey, string bittrexSecret)
        {
            /*var user = await GetUser(userId);

            if (user == null)
            {
                return (null, new { errorMessage = "no such user with id exists" });
            }*/

            var bittrexClient = new Exchange(
                new ExchangeContext
                {
                    ApiKey = bittrexApiKey,
                    Secret = bittrexSecret,
                    Simulate = false,
                });

            var balances = await bittrexClient.GetBalances();
            var nonZeroBalances = balances.Where(o => o.Balance > 2);
            nonZeroBalances = nonZeroBalances.Where(o => o.Currency != "BTC-BTC").ToList();

            var summary = new TradeGuardSummary();

            foreach (var balance in nonZeroBalances)
            {
                await Task.Delay(1005); // delay as a hack around bittrex api rate limiting

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

            return (summary, null);
        }

        static bool IsValidStopLimitOrder(OpenOrder o)
        {
            return
                o.OrderType == OpenOrderType.Limit_Sell &&
                o.Condition == "LESS_THAN" &&
                !string.IsNullOrEmpty(o.ConditionTarget);
            //&& o.Price < currentPrice // need current price!
        }
    }
}