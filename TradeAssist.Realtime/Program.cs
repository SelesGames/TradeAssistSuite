using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Infrastructure;
using Microsoft.Owin.Hosting;
using NPoloniex.API.Push;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeAssist.Realtime
{
    class Program
    {
        static IHubContext hubContext;

        static void Main(string[] args)
        {
            var client = new PoloniexWebSocketClient();
            client.Connect().Wait();
            client.SubscribeToTicker().Wait();
            Ticker.Current.Tick += OnTick;

            using (WebApp.Start<Startup>("http://localhost:1942"))
            {
                Console.WriteLine("Server running");

                var dependencyResolver = GlobalHost.DependencyResolver;
                var connectionManager = dependencyResolver.Resolve<IConnectionManager>();
                hubContext = connectionManager.GetHubContext<TickerHub>();

                Console.ReadLine();
            }
        }

        static void OnTick(object sender, Tick e)
        {
            var targets = hubContext.Clients.Group(e.CurrencyPair);
            var output = new
            {
                currencyPair = e.CurrencyPair.ToString(),
                last = e.Last.ToString(),
            };
            targets.onTick(output);
        }
    }
}
