using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Infrastructure;
using Microsoft.Owin.Hosting;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace TradeAssist.Realtime
{
    /// <summary>
    /// Entry point for the startup of the entire Service
    /// </summary>
    public class ServiceStartup : IOnPriceChangeAction, IOnCandlesticksAction, IDisposable
    {
        IHubContext hubContext;
        IHubContext candleHubContext;
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
                candleHubContext = connectionManager.GetHubContext<CandleHub>();
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

            var candleTracker = CandlestickTracker.Current;
            candleTracker.OnCandlesticksAction = this;

            await candleTracker.Initialize();
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

        public void OnCandlesticks(List<Candle> pc)
        {
            var targets = hubContext?.Clients.Group("oneMinute");
            targets.onTick(pc);
        }
    }
}