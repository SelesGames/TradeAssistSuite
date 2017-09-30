using NBinance.API.Push;
using NBinance.API.Push.Endpoints;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using TradeAssist.Realtime.Ticker;
using TradeAssist.Realtime.Ticker.Intelligence;
using TAT = TradeAssist.Realtime.Trades;
using System.Collections.Generic;

namespace GeneralTester
{
    class Program
    {
        static void Main(string[] args)
        {
            TestArbitration().Wait();
            //TestBinanceTradeEvents().Wait();

            //TestPoloniexTradeListening().Wait();

            Console.ReadLine();
        }

        static async Task TestPoloniexTradeListening()
        {
            var client = TAT.TradeTracker.Current;
            client.OnTradeAction = new OnTradeAction();
            await client.Initialize();
        }

        class OnTradeAction : TAT.IOnTradeAction
        {
            public void OnTrade(TradeAssist.Realtime.Trades.Trade pc)
            {
                var output = $"{pc.CurrencyPair} {pc.Quantity} units at {pc.Price} [{pc.TradeType}]\t\tuet: {pc.UnixEpochTime}";
                Console.WriteLine(output);
            }
        }

        static async Task TestArbitration()
        {
            //await new ArbitrationPriceActionTracker().Begin();

            var priceTracker = PriceTracker.Current;
            await priceTracker.Initialize();

            while (true)
            {
                await Task.Delay(10000);

                var sw = System.Diagnostics.Stopwatch.StartNew();
                var arbOpps = ArbitrationTracker.FindArbitrationOpportunities();
                sw.Stop();

                foreach (var arbOpp in arbOpps)
                    Console.WriteLine(arbOpp);
                Console.WriteLine($"Above calculations took {sw.ElapsedMilliseconds}ms to calculate");
                Console.WriteLine();
            }
        }

        class ArbitrationPriceActionTracker : IOnPriceChangeAction
        {
            SortedSet<string> sortedArbList = new SortedSet<string>();

            public async Task Begin()
            {
                var priceTracker = PriceTracker.Current;
                await priceTracker.Initialize();

                var arbList = ArbitrationTracker.GetMarketsThatExchangeOnBothBtcAndEth(priceTracker.GetMarkets("poloniex"));
                foreach (var item in arbList)
                    sortedArbList.Add(item);

                priceTracker.OnPriceChangeAction = this;
            }

            public void OnPriceChange(PriceChange pc)
            {
                if (sortedArbList.Contains(pc.CurrencyPair))
                {
                    var arbOpp = ArbitrationTracker
                        .FindArbitrationOpportunitiesForExchangeCurrency(
                            pc.Exchange, 
                            getCurrencyPair(pc.CurrencyPair).currency);

                    if (arbOpp != null)
                        Console.WriteLine(arbOpp);
                }

                (string basePair, string currency) getCurrencyPair(string input)
                {
                    var split = input.Split('-');
                    return (split[0], split[1]);
                }
            }
        }

        static async Task TestBinanceTradeEvents()
        {
            var handle = new Trades().Subscribe("ETH-BTC", new OnTrade());
            await Task.Delay(60000);
            handle.Dispose();
        }

        static async Task TestBinanceOrderBookEvents()
        {
            var handle = new OrderBook().Subscribe("ETH-BTC", new OnOrderEntry());
            await Task.Delay(60000);
            handle.Dispose();
        }

        class OnTrade : IOnDataHandler<Trade>
        {
            public void HandleData(Trade obj)
            {
                var serialized = JsonConvert.SerializeObject(obj);
                Console.WriteLine(serialized);
            }
        }

        class OnOrderEntry : IOnDataHandler<OrderBookEvent>
        {
            public void HandleData(OrderBookEvent obj)
            {
                var serialized = JsonConvert.SerializeObject(obj);
                Console.WriteLine(serialized);
            }
        }
    }
}
