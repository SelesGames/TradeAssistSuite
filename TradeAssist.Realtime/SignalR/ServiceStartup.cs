using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Infrastructure;
using Microsoft.Owin.Hosting;
using System;
using System.Threading.Tasks;
using TradeAssist.Realtime.Ticker;

namespace TradeAssist.Realtime.SignalR
{
    /// <summary>
    /// Entry point for the startup of the entire Service
    /// </summary>
    public class ServiceStartup : IOnPriceChangeAction, IDisposable
    {
        IHubContext hubContext;
        IDisposable singalRStartupDisposeHandle;
        bool isDisposed;

        public async Task Startup(string address)
        {
            try
            {
                singalRStartupDisposeHandle = WebApp.Start<WebAppStartup>(address);
                Console.WriteLine("Server running");

                var dependencyResolver = GlobalHost.DependencyResolver;
                var connectionManager = dependencyResolver.Resolve<IConnectionManager>();
                hubContext = connectionManager.GetHubContext<TickerHub>();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error starting SignalR service");
                Console.WriteLine(ex);
                Dispose();
            }

            if (isDisposed)
                return;

            var priceTracker = PriceTracker.Current;
            priceTracker.OnPriceChangeAction = this;

            await priceTracker.Initialize();
        }

        public void OnPriceChange(PriceChange pc)
        {
            var targets = hubContext?.Clients.Group(pc.CurrencyPair);
            var output = new object[] { pc.CurrencyPair, pc.VolumeWeightedPrice, pc.Exchange, pc.ExchangePrice };
            targets.onTick(output);
        }

        public void Dispose()
        {
            if (isDisposed)
                return;

            isDisposed = true;
            singalRStartupDisposeHandle?.Dispose();
        }
    }
}