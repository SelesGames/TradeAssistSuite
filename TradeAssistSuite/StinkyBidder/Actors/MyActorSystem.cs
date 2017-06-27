using Akka.Actor;
using NPoloniex.API;
using NPoloniex.API.Push;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradeAssist.Suite.StinkyBidder;
using TradeAssist.Suite.StinkyBidder.Actors;

namespace TradeAssist.Suite
{
    /// <summary>
    /// Entry point for the actor system, including all initialization
    /// </summary>
    public class MyActorSystem
    {
        const string ActorSystemName = "myActorSystem";
        const string MonitorName = "monitor";
        const string ExecutorName = "executor";

        public static MyActorSystem Current = new MyActorSystem();

        Dictionary<CurrencyPair, IActorRef> tickRouterLookup;// = new Dictionary<CurrencyPair, IActorRef>();

        public static string MonitorNamingConvention(CurrencyPair currencyPair) =>
            $"{currencyPair}-Monitor";

        private MyActorSystem() { }

        ActorSystem actorSystem;
        IActorRef executor;

        public IActorRef GetExecutor() => executor;
        public IActorRef GetMonitor(CurrencyPair currencyPair) => tickRouterLookup[currencyPair];

        public async Task Initialize()
        {
            actorSystem = ActorSystem.Create(ActorSystemName);

            await Markets.Initialize();
            var currencyMarkets = Markets.CurrencyPairs;

            tickRouterLookup = new Dictionary<CurrencyPair, IActorRef>();

            foreach (var item in currencyMarkets.Select(o => o.Value))
            {
                var currencyPair = item;
                var actor = actorSystem.ActorOf(
                    Props.Create(() =>
                        new Monitor(currencyPair)),
                        MonitorNamingConvention(currencyPair));

                tickRouterLookup.Add(currencyPair, actor);
            }

            var bridge = new TickToMonitorBridge(tickRouterLookup);
            await bridge.Initialize();

            executor = actorSystem.ActorOf(Props.Create(() => new Executor()), "executor");
        }
    }
}