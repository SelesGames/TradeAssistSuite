using System;
using System.Threading.Tasks;
using Poloniex = NPoloniex.API.Push;

namespace TradeAssist.Realtime.Trades
{
    public class TradeTracker : Poloniex.IOnTradeAction
    {
        public static TradeTracker Current { get; } = new TradeTracker();
        private TradeTracker() { }

        public IOnTradeAction OnTradeAction { get; set; }

        public async Task Initialize()
        {
            await InitializePoloniex();
            Console.WriteLine("poloniex initialized");

            /*await InitializeBittrex();
            Console.WriteLine("bittrex initialized");

            async Task InitializeBittrex()
            {
                var bittrexHub = new NBittrex.BittrexHub();
                bittrexHub.OnUpdateSummaryStateTick += (_, tick) =>
                    {
                        if (tick == null || tick.Deltas == null)
                            return;

                        foreach (var item in tick.Deltas)
                        {
                            OnPriceEvent(
                                currencyPair: Canonical(item.MarketName, BittrexSeparator), 
                                exchange: Bittrex, 
                                price: item.Last, 
                                volume: item.BaseVolume,
                                bid: item.Bid,
                                ask: item.Ask);
                        }
                    };

                await bittrexHub.Connect();
            }*/

            async Task InitializePoloniex()
            {
                await Poloniex.Markets.Initialize();
                var client = new Poloniex.PoloniexWebSocketClient
                {
                    OnTradeAction = this
                };

                await client.Connect();
                //await client.SubscribeToTrades("BTC_ETH"); // for testing;
                await client.SubscribeToTradesForAllCurrencies();
            }
        }

        void Poloniex.IOnTradeAction.OnPriceChange(Poloniex.Trade trade)
        {
            OnTradeAction?.OnTrade(new Trade
            {
                CurrencyPair = trade.CurrencyPair,
                TradeId = trade.TradeId,
                TradeType = (TradeType)(int)trade.TradeType,
                Price = trade.Price,
                Quantity = trade.Quantity,
                UnixEpochTime = trade.UnixEpochTime,
            });
        }
    }
}