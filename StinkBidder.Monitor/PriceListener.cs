using NPoloniex.API.Push;
using System;
using System.Threading.Tasks;

namespace StinkBidder.Monitor
{
    public static class PriceListener
    {
        enum BootupStatus { None, Initializing, Initialized };

        static BootupStatus status = BootupStatus.None;
        static Users users;

        public static async Task Initialize()
        {
            if (status == BootupStatus.Initializing || status == BootupStatus.Initialized)
                return;

            status = BootupStatus.Initializing;

            // boot up the Poloniex websocket client
            var client = new PoloniexWebSocketClient();
            await client.Connect();
            await client.SubscribeToTicker();

            // boot up the user list
            users = await LoadUsers();

            Ticker.Current.Subscribe("BTC_ETH", ProcessTick);

            status = BootupStatus.Initialized;
        }

        static async Task<Users> LoadUsers()
        {
            var users = new Users();
            var mockUser = new User
            {
                PublicKey = "F5QR8MJE-HN5LH4WJ-8X9758YH-NDLRE7NJ",
                PrivateKey = "0be35048de6102dfa9927504b4099aac222636f2dd96983f9713fe0c9b93d489f38ae08d9e3e3b4b3509ef77c182f9000a4b8b21c49d8af84ad0863c6937f932",
            };
            
            var mockStinkBid = new StinkBid
            {
                AmountInBtc = 5m,
                BuyPrice = 0.00000001m,
                CurrencyPair = "BTC_ETH",
            };
            mockUser.StinkBids.StinkBids.Add(mockStinkBid.CurrencyPair, mockStinkBid);

            users.Add(mockUser);
            return await Task.FromResult(users);
        }

        static void ProcessTick(Tick tick)
        {
            var key = tick.CurrencyPair;
            var last = tick.Last;

            foreach (var user in users)
            {
                if (user.StinkBids.StinkBids.ContainsKey(key))
                {
                    var stinkBid = user.StinkBids.StinkBids[key];

                    Console.WriteLine(
                        "User has a stinkBid for {0}: currentPrice: {1}, stinkPrice: {2}",
                        key, last, stinkBid.BuyPrice);

                    if (stinkBid.OrderStatus == StinkBidOrderStatus.None &&
                        last <= stinkBid.BuyPrice)
                    {
                        QueueOrder(user, stinkBid);
                    }
                }
            }
        }

        //TODO: ideally this should send the order to a queue service that actually makes the API calls
        static async void QueueOrder(User user, StinkBid stinkBid)
        {
            stinkBid.OrderStatus = StinkBidOrderStatus.Queued;

            var orderNumber = await TradeSubmitter.ExecuteStinkBid(user, stinkBid);
            if (orderNumber > 0)
            {
                stinkBid.OrderStatus = StinkBidOrderStatus.Submitted;
                Console.WriteLine(string.Format("Stinkbid submitted for {0} at {1}", stinkBid.CurrencyPair, stinkBid.BuyPrice));
            }
        }
    }
}