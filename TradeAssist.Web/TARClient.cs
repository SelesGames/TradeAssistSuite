using Microsoft.AspNet.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TradeAssist.Web
{
    public class TARClient
    {
        public static TARClient Current { get; } = new TARClient();
        private TARClient() { }

        IHubProxy hubProxy;

        readonly int retryIntervalInMilliseconds = 1000;

        public async Task Initialize(string connectionString)
        {
            var hubConnection = new HubConnection(connectionString);
            hubProxy = hubConnection.CreateHubProxy("ticker");

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

        public async Task<List<string>> Markets(string exchange = null)
        {
            var markets = await hubProxy.Invoke<List<string>>("markets", exchange);
            return markets;
        }
    }
}
