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

            Console.ReadLine();
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

            await hubProxy.Invoke("subscribe", currencyPair);
        }

        void OnData(dynamic d)
        {
            Console.WriteLine(d);
        }
    }
}
