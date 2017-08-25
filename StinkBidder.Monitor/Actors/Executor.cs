using Akka.Actor;
using NPoloniex.API.Http;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace StinkBidder.Monitor.Actors
{
    public class Executor : ReceiveActor
    {
        public Executor()
        {
            Receive<StinkBid>(OnStinkBidReceived);
        }

        Task<User> GetUser(string userId)
        {
            var mockUser = new User
            {
                PublicKey = "F5QR8MJE-HN5LH4WJ-8X9758YH-NDLRE7NJ",
                PrivateKey = "0be35048de6102dfa9927504b4099aac222636f2dd96983f9713fe0c9b93d489f38ae08d9e3e3b4b3509ef77c182f9000a4b8b21c49d8af84ad0863c6937f932",
            };
            return Task.FromResult(mockUser);
        }

        async Task<ulong> ExecuteOrder(StinkBid stinkBid)
        {
            // time roundtrip for getting wallet balance
            try
            {
                var sw = Stopwatch.StartNew();

                // TODO: update DB with stinkbid order status update to "Queued"
                var user = await GetUser(stinkBid.UserId);
                var auth = new Authenticator(user.PublicKey, user.PrivateKey);
                var client = new ApiHttpClient(auth);

                var balances = await client.Wallet.GetCompleteBalances();
                var btcBalance = balances["BTC"];
                var availableBtc = btcBalance.Available;

                var allowance = Math.Min(stinkBid.AmountInBtc, availableBtc);
                var rate = stinkBid.BuyPrice;
                var purchaseAmount = Math.Round(allowance / rate, 8);

                sw.Stop();
                var balanceRetrievalTime = sw.Elapsed;

                sw.Restart();
                var orderNumber = await client.Trading.Buy(stinkBid.CurrencyPair, rate, purchaseAmount);
                //TODO: update DB with stinkbid order status update to "Submitted"
                sw.Stop();

                var orderExecutiontime = sw.Elapsed;

                Console.WriteLine($"Order exec: User: {stinkBid.UserId} at {rate} for {purchaseAmount} units");
                Console.WriteLine($"diagnosits: balance retrieval: {balanceRetrievalTime}  orderExecution: {orderExecutiontime}\r\n\r\n");
                return orderNumber;
            }
            catch(Exception ex)
            {
                Console.Write(ex);
                return 0;
            }
        }

        bool OnStinkBidReceived(StinkBid stinkBid)
        {
            if (stinkBid == null)
                return false;

            ExecuteOrder(stinkBid).PipeTo(Self);

            return true;
        }

        object DoSomethingWithOrderNumber(ulong orderNumber)
        { return null; }

        object OnOrderExecutionFailure(Exception exception)
        { return null; }
    }
}
