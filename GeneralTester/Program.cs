using NBinance.API.Push;
using NBinance.API.Push.Endpoints;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneralTester
{
    class Program
    {
        static void Main(string[] args)
        {
            TestTrades().Wait();

            Console.ReadLine();
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
