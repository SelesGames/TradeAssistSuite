using NPoloniex.API;
using NPoloniex.API.Http;
using NPoloniex.API.Push;
using System;
using System.Linq;
using System.Threading.Tasks;
using static TradeAssist.Suite.PrintHelper;
using Akka.Actor;

namespace TradeAssist.Suite
{
    class Program
    {
        static void Main(string[] args)
        {
            var bittrexHub = new NBittrex.BittrexHub();
            bittrexHub.Initialize().Wait();
            bittrexHub.SubscribeToTicker("BTC-ANS").Wait();
            //bittrexHub.SubscribeToFlood().Wait();
            Console.ReadLine();

            return;


            MyActorSystem.Current.Initialize().Wait();

            decimal triggerPrice = 0.00389m;

            var stinkBids = Enumerable.Range(1, 1).Select(o =>
                new StinkBid
                {
                    AmountInBtc = 0.0001m,
                    BuyPrice = triggerPrice - (decimal)o * 0.000001m,
                    TriggerPrice = triggerPrice - (decimal)o * 0.000001m,
                    CurrencyPair = "BTC_STRAT",
                    UserId = o,
                });

            var monitorRef = MyActorSystem.Current.GetMonitor("BTC_STRAT");
            foreach (var item in stinkBids)
            {
                monitorRef.Tell(item);
            }

            /*Console.WriteLine("Enter a command...");
            while(true)
            {
                ReadInput(Console.ReadLine());
            }*/

            Console.ReadLine();
        }

        static async void ReadInput(string v)
        {
            var tokens = v.Split(' ');
            var command = tokens.First();

            switch (command)
            {
                case "check":
                    await CheckCurrency(tokens);
                    break;

                case "start":
                    await Start(tokens);
                    break;

                default:
                    Console.WriteLine("Unrecognized command");
                    break;
            }
        }

        static async Task Start(string[] tokens)
        {
            try
            {
                var input = tokens[1];
                if (input == "ticker")
                {
                    await Ticker.Current.Initialize();
                    /*PoloniexWebSocketClient client = new PoloniexWebSocketClient();
                    await client.Connect();
                    await client.SubscribeToTicker();*/
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Command failed:");
                Console.WriteLine(ex);
            }
        }

        static async Task CheckCurrency(string[] tokens)
        {
            try
            {
                var currency = tokens[1];
                await GetTradeHistory(currency);
            }
            catch(Exception ex)
            {
                Console.WriteLine("Command failed:");
                Console.WriteLine(ex);
            }
        }

       /* static async Task TestHarness()
        {
            //await StartWebSocketClient();
            //Thread.Sleep(5);
            await GetTradeHistory();
        }*/

        /*static async Task StartWebSocketClient()
        {
            PoloniexWebSocketClient client = new PoloniexWebSocketClient();
            await client.Connect();
            await client.SubscribeToTicker();

            Ticker.Current.Subscribe("BTC_ETH", ProcessTick);
        }*/

        /*static void ProcessTick(Tick tick)
        {
            var output = new object[] { tick.CurrencyPair.ToString(), tick.Last, tick.HighestBid, tick.LowestAsk };
            var outputString = JsonConvert.SerializeObject(output, Formatting.None);
            Console.WriteLine(outputString);
        }

        static async Task TryStinkyBidder()
        {
            await PriceListener.Initialize();
        }*/

        static async Task GetTradeHistory(string currency)
        {
            currency = currency.ToUpper();

            CurrencyPair currencyPair;

            if (CurrencyPair.IsCurrencyPair(currency))
                currencyPair = currency;
            else
                currencyPair = CurrencyPair.PrependBitcoin(currency);


            var authenticator = new Authenticator(
                "F5QR8MJE-HN5LH4WJ-8X9758YH-NDLRE7NJ",
                "0be35048de6102dfa9927504b4099aac222636f2dd96983f9713fe0c9b93d489f38ae08d9e3e3b4b3509ef77c182f9000a4b8b21c49d8af84ad0863c6937f932");
            var client = new ApiHttpClient(authenticator);

            var trades = await client.Trading.GetTrades(currencyPair, DateTime.Parse("1/1/2008"), DateTime.UtcNow);

            var balances = await client.Wallet.GetCompleteBalances();
            //var currentTick = Ticker.Current[currencyPair];
            //var currentPrice = currentTick.Last;

            var balance = balances[currencyPair.QuoteCurrency];
            var balanceSize = balance.Available + balance.OnOrders;
            var currentValue = balance.BtcValue;

            var calculatedCurrentPrice = balance.BtcValue / balanceSize;
            var roundedCalculatedCurrentPrice = Math.Round(calculatedCurrentPrice, 8);

            var positionHistory = PositionCalculator.ExtractPositions(balanceSize, roundedCalculatedCurrentPrice, trades);

            PrettyPrint(positionHistory.Open);
        }
    }
}