using System.Collections.Generic;
using static System.Math;


namespace TradeAssist.Realtime.Ticker
{
    /// <summary>
    /// Stores prices across exchanges for a particular currency - calculates VolumeWeightedPrice
    /// </summary>
    public class PriceInfo
    {
        public string CurrencyPair { get; }
        public decimal VolumeWeightedPrice { get; private set; }
        public IDictionary<string, ExchangePriceInfo> Prices { get; }
            = new Dictionary<string, ExchangePriceInfo>();

        public PriceInfo(string currencyPair)
        {
            CurrencyPair = currencyPair;
        }

        public void AdjustForLatestPrice(string exchange, ExchangePriceInfo exchangePriceInfo)
        {
            Prices[exchange] = exchangePriceInfo;

            decimal volumeWeightedPriceTotal = 0m;
            decimal volumeTotal = 0m;

            foreach (var p in Prices)
            {
                var (price, volume) = (p.Value.Price, p.Value.Volume);
                volumeWeightedPriceTotal += price * volume;
                volumeTotal += volume;
            }

            VolumeWeightedPrice = Round(volumeWeightedPriceTotal / volumeTotal, 8);
        }

        /*
        5 * 2.3 + 33 * 3.3 = 11.5 + 108.9 = 120.4 / 38 = 3.16842105 [2 multiplies, 1 divide]

            vs

        5/38 * 2.3 + 33/38 * 3.3 = 0.30263158 + 2.86578947 = 3.16842105 [ 2 divides, 2 multiplies]
        */
    }
}