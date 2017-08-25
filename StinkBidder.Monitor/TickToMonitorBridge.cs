using Akka.Actor;
using NPoloniex.API;
using NPoloniex.API.Push;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StinkBidder.Monitor
{
    class TickToMonitorBridge
    {
        readonly Dictionary<CurrencyPair, IActorRef> tickRouterLookup;

        public TickToMonitorBridge(Dictionary<CurrencyPair, IActorRef> tickRouterLookup)
        {
            this.tickRouterLookup = tickRouterLookup;
        }

        public async Task Initialize()
        {
            var client = new PoloniexWebSocketClient();
            await client.Connect();
            await client.SubscribeToTicker();

            foreach (var currencyPair in tickRouterLookup.Select(o => o.Key))
            {
                Ticker.Current.Subscribe(currencyPair, ProcessTick);
            }
        }

        void ProcessTick(Tick tick)
        {
            var currencyPair = tick.CurrencyPair;
            if (tickRouterLookup.TryGetValue(currencyPair, out IActorRef monitorRef))
            {
                monitorRef.Tell(tick);
            }
        }
    }
}