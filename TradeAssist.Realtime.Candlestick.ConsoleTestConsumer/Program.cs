using Microsoft.AspNet.SignalR.Client;
using System;
using System.Threading.Tasks;

namespace TradeAssist.Realtime.Candlestick.ConsoleTestConsumer
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new TARClient();
            client.Initialize().Wait();
            Console.ReadLine();
        }
    }

    public class TARClient
    {
        string ConnectionString = "http://localhost:1943";

        IHubProxy candleHubProxy;

        readonly int retryIntervalInMilliseconds = 1000;

        public async Task Initialize()
        {
            var hubConnection = new HubConnection(ConnectionString);

            candleHubProxy = hubConnection.CreateHubProxy("candle");
            candleHubProxy.On("onTick", OnData);

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

            await candleHubProxy.Invoke("subscribe", "BTC-ANS");
        }

        void OnData(dynamic d)
        {
            Console.WriteLine(d);
        }
    }
}