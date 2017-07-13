using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Infrastructure;
using Microsoft.Owin.Hosting;
using System;
using System.Threading.Tasks;

namespace TradeAssist.Realtime
{
    public class ServiceStartup : IOnPriceChangeAction, IDisposable
    {
        IHubContext hubContext;
        IDisposable singalRStartupDisposeHandle;
        bool isDisposed;

        public async Task Startup(string address)
        {
            try
            {
                singalRStartupDisposeHandle = WebApp.Start<Startup>(address);
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

            var apiListener = new ApiListener();
            apiListener.OnPriceChangeAction = this;

            await apiListener.Initialize();
        }

        public void OnPriceChange(PriceChange pc)
        {
            var targets = hubContext?.Clients.Group(pc.CurrencyPair);
            var output = new
            {
                currencyPair = pc.CurrencyPair.ToString(),
                last = pc.AveragedLastPrice
            };
            targets.onTick(output);
        }

        public void Dispose()
        {
            isDisposed = true;
            singalRStartupDisposeHandle?.Dispose();
        }
    }
}