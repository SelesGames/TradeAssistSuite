using Microsoft.AspNet.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TradeAssist.Realtime.ConsoleTestConsumer
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new TARClient();

            client.Initialize().Wait();

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

                else if (tokens.First() == "m")
                    client.Markets().Wait();
            }
        }
    }

    public class TARClient
    {
        //string ConnectionString = "http://cba0978522d7:1942";
        //string ConnectionString = "http://tradeassistrealtime.azurewebsites.net:1942";
        string ConnectionString = "http://tradeassistrealtime.azurewebsites.net/api/continuouswebjobs/tradeassist-realtime:1942";

        IHubProxy hubProxy;

        readonly int retryIntervalInMilliseconds = 1000;

        public async Task Initialize()
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
                    Console.WriteLine("Connected");
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

        public async Task Markets(string exchange = null)
        {
            var markets = await hubProxy.Invoke<List<string>>("markets", exchange);
            Console.WriteLine(markets.Count);
        }
    }
}