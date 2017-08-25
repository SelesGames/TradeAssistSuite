using System.Collections.Generic;
using static System.Math;


namespace TradeAssist.Realtime.Ticker
{
    /// <summary>
    /// Stores prices across exchanges for a particular currency - calculates VolumeWeightedPrice
    /// </summary>
    public class PriceInfo
    {
        public decimal VolumeWeightedPrice { get; private set; }
        public Dictionary<string, (decimal price, decimal volume)> Prices { get; }
            = new Dictionary<string, (decimal, decimal)>();

        public void AdjustForLatestPrice(string exchange, decimal exchangePrice, decimal exchangeVolume)
        {
            Prices[exchange] = (exchangePrice, exchangeVolume);

            decimal volumeWeightedPriceTotal = 0m;
            decimal volumeTotal = 0m;

            foreach (var p in Prices)
            {
                var (price, volume) = p.Value;
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