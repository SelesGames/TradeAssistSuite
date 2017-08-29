using NBinance.API.Push;
using NBinance.API.Push.Endpoints;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using TradeAssist.Realtime.Ticker;
using TradeAssist.Realtime.Ticker.Intelligence;
using TAT = TradeAssist.Realtime.Trades;

namespace GeneralTester
{
    class Program
    {
        static void Main(string[] args)
        {
            //TestArbitration().Wait();
            //TestTrades().Wait();

            TestPoloniexTradeListening().Wait();

            Console.ReadLine();
        }

        static async Task TestPoloniexTradeListening()
        {
            var client = TAT.TradeTracker.Current;
            client.OnTradeAction = new OnTradeAction();
            await client.Initialize();
        }

        class OnTradeAction : TAT.IOnTradeAction
        {
            public void OnTrade(TradeAssist.Realtime.Trades.Trade pc)
            {
                var output = $"{pc.CurrencyPair} {pc.Quantity} units at {pc.Price} [{pc.TradeType}]\t\tuet: {pc.UnixEpochTime}";
                Console.WriteLine(output);
            }
        }

        static async Task TestArbitration()
        {
            var priceTracker = PriceTracker.Current;
            await priceTracker.Initialize();



            while (true)
            {
                await Task.Delay(10000);

                var sw = System.Diagnostics.Stopwatch.StartNew();
                var arbOpps = ArbitrationTracker.FindArbitrationOpportunities();
                sw.Stop();

                foreach (var arbOpp in arbOpps)
                    Console.WriteLine(arbOpp);
                Console.WriteLine($"Above calculations took {sw.ElapsedMilliseconds}ms to calculate");
                Console.WriteLine();
            }
        }

        static async Task TestTrades()
        {
            //var handle = new Trades().Subscribe("ETH-BTC", new OnTrade());
            var handle = new OrderBook().Subscribe("ETH-BTC", new OnOrderEntry());

            await Task.Delay(60000);

            handle.Dispose();
        }

        class OnTrade : IOnDataHandler<Trade>
        {
            public void HandleData(Trade obj)
            {
                var serialized = JsonConvert.SerializeObject(obj);
                Console.WriteLine(serialized);
            }
        }

        class OnOrderEntry : IOnDataHandler<OrderBookEvent>
        {
            public void HandleData(OrderBookEvent obj)
            {
                var serialized = JsonConvert.SerializeObject(obj);
                Console.WriteLine(serialized);
            }
        }
    }
}
