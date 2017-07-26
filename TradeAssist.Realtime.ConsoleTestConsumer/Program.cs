using Microsoft.AspNet.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeAssist.Realtime.ConsoleTestConsumer
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new TARClient();

            string currencyPair = null;
            if (args.Any())
                currencyPair = args.First();
            else
                currencyPair = "BTC_ETH";

            client.Initialize(currencyPair).Wait();

            while (true)
            {
                var input = Console.ReadLine();
                var tokens = input?.Split(' ').ToList();

                if (tokens.First() == "p")
                    client.PrintPrice(tokens.Skip(1).First()).Wait();

                else if (tokens.First() == "s")
                    client.Subscribe(tokens.Skip(1).First()).Wait();

                else if (tokens.First() == "u")
                    client.Unsubscribe(tokens.Skip(1).First()).Wait();
            }
        }
    }

    public class TARClient
    {
        string ConnectionString = "http://localhost:1942";
        IHubProxy hubProxy;

        readonly int retryIntervalInMilliseconds = 1000;

        public async Task Initialize(string currencyPair = "BTC_ETH")
        {
            var hubConnection = new HubConnection(ConnectionString);
            hubProxy = hubConnection.CreateHubProxy("ticker");
            hubProxy.On("onTick", OnData);

            bool connected = false;

            while (!connected)
            {
                try
                {
                    await hubConnection.Start();
                    connected = true;
                }
                catch { }

                if (!connected)
                    await Task.Delay(retryIntervalInMilliseconds);
            }
        }

        void OnData(dynamic d)
        {
            Console.WriteLine(d);
        }

        public async Task Subscribe(string currencyPair)
        {
            await hubProxy.Invoke("subscribe", currencyPair);
        }

        public async Task Unsubscribe(string currencyPair)
        {
            await hubProxy.Invoke("unsubscribe", currencyPair);
        }

        public async Task PrintPrice(string currencyPair)
        {
            var price = await hubProxy.Invoke<dynamic>("last", currencyPair);
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(price));
        }
    }
}