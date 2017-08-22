using NBittrex.API.Http;
using System.Linq;
using System.Threading.Tasks;

namespace TradeGuard.Logic
{
    public static partial class Functions
    {
        public static async Task<(TradeGuardSummary summary, dynamic error)> GetBalances(string userId)
        {
            var user = await GetUser(userId);

            if (user == null)
            {
                return (null, new { errorMessage = "no such user with id exists" });
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

            return (summary, null);
        }

        static bool IsValidStopLimitOrder(OpenOrder o)
        {
            return o.OrderType == OpenOrderType.Limit_Sell;
            //&& o.Price < currentPrice // need current price!
        }
    }
}