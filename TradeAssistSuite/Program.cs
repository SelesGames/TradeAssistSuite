using Akka.Actor;
using NPoloniex.API.Push;
using StinkBidder.Monitor;
using StinkBidder.Monitor.Actors;
using System;
using System.Linq;
using System.Threading.Tasks;
using TradeInsights.Logic.Poloniex.Positions;
using static TradeInsights.Logic.Poloniex.Positions.PrintHelper;

namespace TradeAssistSuite
{
    class Program
    {
        static void Main(string[] args)
        {
            var commandList = new CommandList();
            while (true)
            {
                Console.Write("> ");
                var command = Console.ReadLine();
                commandList.ProcessCommand(command);
                Console.WriteLine();
            }


            /*var bittrexHub = new NBittrex.BittrexHub();
            bittrexHub.Initialize().Wait();
            bittrexHub.SubscribeToTicker("BTC-ANS").Wait();
            //bittrexHub.SubscribeToFlood().Wait();
            Console.ReadLine();*/

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
                    UserId = o.ToString(),
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
            var positionHistory = await TradeInsights.Logic.Poloniex.Functions.GetPositionHistory(currency);
            positionHistory.Open.PrettyPrint();
        }
    }
}