using Akka.Actor;
using NPoloniex.API.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeAssistSuite.StinkyBidder.Actors
{
    public class Executor : ReceiveActor
    {
        public Executor()
        {
            Receive<StinkBid>(OnStinkBidReceived);
        }

        Task<User> GetUser(UserId userId)
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
            // TODO: update DB with stinkbid order status update to "Queued"
            var user = await GetUser(stinkBid.UserId);
            var auth = new Authenticator(user.PublicKey, user.PrivateKey);
            var client = new ApiHttpClient(auth);

            var balances = await client.Wallet.GetCompleteBalances();
            var btcBalance = balances["BTC"];
            var availableBtc = btcBalance.Available;

            var allowance = Math.Min(stinkBid.Amount, availableBtc);
            var rate = stinkBid.BuyPrice;
            var purchaseAmount = Math.Round(allowance / rate, 8);

            var orderNumber = await client.Trading.Buy(stinkBid.CurrencyPair, rate, purchaseAmount);
            //TODO: update DB with stinkbid order status update to "Submitted"

            return orderNumber;
        }

        bool OnStinkBidReceived(StinkBid stinkBid)
        {
            if (stinkBid == null)
                return false;

            ExecuteOrder(stinkBid).PipeTo(Self, Self, DoSomethingWithOrderNumber, OnOrderExecutionFailure);

            return true;
        }

        object DoSomethingWithOrderNumber(ulong orderNumber)
        { return null; }

        object OnOrderExecutionFailure(Exception exception)
        { return null; }
    }
}
