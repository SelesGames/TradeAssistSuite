using Akka.Actor;
using NPoloniex.API.Push;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradeAssistSuite.StinkyBidder.Actors;

namespace TradeAssistSuite
{
    /// <summary>
    /// Entry point for the actor system, including all initialization
    /// </summary>
    public class MyActorSystem
    {
        const string ActorSystemName = "myActorSystem";
        const string MonitorRef = "monitor";
        const string ExecutorRef = "executor";

        public static MyActorSystem Current = new MyActorSystem();

        private MyActorSystem() { }

        ActorSystem actorSystem;

        public IActorRef GetExecutor() => actorSystem.ActorOf<Executor>(ExecutorRef);
        //public ActorSelection Executor => actorSystem.ActorSelection(ExecutorRef);

        public async Task Initialize()
        {
            actorSystem = ActorSystem.Create("myActorSystem");

            await Markets.Initialize();
            var currencyMarkets = Markets.CurrencyPairs;

            foreach (var item in currencyMarkets.Select(o => o.Value))
            {
                var currencyPair = item;
                var actor = actorSystem.ActorOf(Props.Create(() => new Monitor(currencyPair)), $"{currencyPair}-Monitor");
            }

            //TODO: create a bridge class between the npoloniex socket price listener and something that fires off Tick events
        }
    }
}