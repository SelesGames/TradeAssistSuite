using Newtonsoft.Json;
using NPoloniex.API.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeAssistSuite
{
    public static class TradeSubmitter
    {
        public static async Task<ulong> ExecuteStinkBid(User user, StinkBid stinkBid)
        {
            var publicKey = user.PublicKey;
            var privateKey = user.PrivateKey;

            var authenticator = new Authenticator(publicKey, privateKey);
            var client = new ApiHttpClient(authenticator);

            var balances = await client.Wallet.GetCompleteBalances();
            var btcBalance = balances["BTC"];
            var availableBtc = btcBalance.Available;

            var allowance = Math.Min(stinkBid.Amount, availableBtc);
            var rate = stinkBid.BuyPrice;
            var purchaseAmount = Math.Round(allowance / rate, 8);

            var orderNumber = await client.Trading.Buy(stinkBid.CurrencyPair, rate, purchaseAmount);

            return orderNumber;

            //var isCancelled = await client.Trading.CancelOrder(stinkBid.CurrencyPair, orderNumber);

            //Console.WriteLine(isCancelled);
        }

        public static async Task ExecuteStinkBid()
        {
            var authenticator = new Authenticator(
                "F5QR8MJE-HN5LH4WJ-8X9758YH-NDLRE7NJ",
                "0be35048de6102dfa9927504b4099aac222636f2dd96983f9713fe0c9b93d489f38ae08d9e3e3b4b3509ef77c182f9000a4b8b21c49d8af84ad0863c6937f932");
            var client = new ApiHttpClient(authenticator);

            var balances = await client.Wallet.GetCompleteBalances();
            var btcBalance = balances["BTC"];
            var availableBtc = btcBalance.Available;

            var stinkBid = new StinkBid
            {
                Amount = 5m,
                BuyPrice = 0.00000001m,
                CurrencyPair = "BTC_ETH",
            };

            var allowance = Math.Min(stinkBid.Amount, availableBtc);
            var rate = stinkBid.BuyPrice;
            var purchaseAmount = Math.Round(allowance / rate, 8);

            var orderNumber = await client.Trading.Buy(stinkBid.CurrencyPair, rate, purchaseAmount);

            var isCancelled = await client.Trading.CancelOrder(stinkBid.CurrencyPair, orderNumber);

            Console.WriteLine(isCancelled);
        }
    }
}