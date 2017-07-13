using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NPoloniex.API.Push;
using NPoloniex.API;

namespace TradeAssist.Realtime
{
    class VolumeWeightedPrice
    {
        CurrencyPair currencyPair;
        Tick lastPoloniexTick;
        Tick lastBittrexTick;

        // Accomodate a price change
        public void Recalculate(Tick tick)
        {

        }
    }

    interface IOnPriceChangeAction
    {
        void OnPriceChange(PriceChange pc);
    }

    public class PriceChange
    {
        public CurrencyPair CurrencyPair { get; set; }
        public decimal AveragedLastPrice { get; set; }
    }

    class ApiListener
    {
        Ticker poloniexTicker = Ticker.Current;
        Dictionary<string, decimal> previousPriceLookup = new Dictionary<string, decimal>();

        public IOnPriceChangeAction OnPriceChangeAction { get; set; }

        public async Task Initialize()
        {
            poloniexTicker.Tick += OnTick;
            await poloniexTicker.Initialize();

            var client = new PoloniexWebSocketClient();
            await client.Connect();
            await client.SubscribeToTicker();
        }

        void OnTick(object sender, Tick e)
        {
            var previousPrice = previousPriceLookup.TryGetValue(e.CurrencyPair, out var val) ? val : -1;
            if (e.Last == previousPrice)
                return;

            previousPriceLookup[e.CurrencyPair] = e.Last;

            var priceChange = new PriceChange
            {
                CurrencyPair = e.CurrencyPair,
                AveragedLastPrice = e.Last,
            };

            OnPriceChangeAction.OnPriceChange(priceChange);
        }
    }
}