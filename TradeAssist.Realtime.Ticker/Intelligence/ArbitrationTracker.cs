using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace TradeAssist.Realtime.Ticker.Intelligence
{
    public class ArbitrationTracker
    {
        public static List<string> FindArbitrationOpportunities()
        {
            var exchanges = PriceTracker.Current.exchangeLookup.ToList();
            //var exchanges = PriceTracker.Current.SelectMany(o => o.Prices).ToDictionary(o => o.Value.Exchange);


            List<string> arbitrationOpportunities = new List<string>();

            foreach (var exchange in exchanges)
            {
                var btcEth = exchange.Value["BTC-ETH"];

                var groups = exchange.Value
                    .Select(o => new { cp = GetCurrencyPair(o.Key), o })
                    .GroupBy(o => o.cp.currency)
                    .Where(o => o.Count() > 1)
                    .ToList();

                foreach (var grouping in groups)
                {
                    var set = grouping.Where(o => o.cp.basePair == "BTC" || o.cp.basePair == "ETH").ToList();

                    var btcVal = set.SingleOrDefault(o => o.cp.basePair == "BTC")?.o.Value;
                    var ethVal = set.SingleOrDefault(o => o.cp.basePair == "ETH")?.o.Value;

                    var (strategy, arbScore) = getArbitrationOpportunityStrategy(btcVal, ethVal, btcEth);
                    var arbScorePercent = Math.Round(arbScore * 100, 2);
                    if (strategy == "buyWithBtc")
                        arbitrationOpportunities.Add(
                            $"[{exchange.Key.ToUpper()}] Buy {grouping.Key} with BTC at {btcVal.LowestAsk} and sell for ETH at {ethVal.HighestBid} for a {arbScorePercent}% yield");
                    else if (strategy == "buyWithEth")
                        arbitrationOpportunities.Add(
                            $"[{exchange.Key.ToUpper()}] Buy {grouping.Key} with ETH at {ethVal.LowestAsk} and sell for BTC at {btcVal.HighestBid} for a {arbScorePercent}% yield");
                }
            }

            return arbitrationOpportunities;

            (string basePair, string currency) GetCurrencyPair(string input)
            {
                var split = input.Split('-');
                return (split[0], split[1]);
            }
            
            (string strategy, double yield) getArbitrationOpportunityStrategy(ExchangePriceInfo btc, ExchangePriceInfo eth, ExchangePriceInfo btcEth)
            {
                if (btc == null || eth == null)
                    return ("none", -1d);

                // what are the results if buying this token with BTC and selling with ETH
                // sequence of events with example NEO
                // 1. Buy NEO with BTC, at the lowest ask price
                // 2. Sell NEO for ETH, at the highest bid price
                // 3. Sell the ETH for BTC, at the highest BTC-ETH bid price
                // The above only makes sense if the pairing converts at a lower price than what you can sell BTC-ETH at
                var buyingBtcPrice = btc.LowestAsk;
                var sellingEthPrice = eth.HighestBid;

                var convertedEthPrice = buyingBtcPrice / sellingEthPrice;
                var currentEthPrice = btcEth.HighestBid;

                if (convertedEthPrice < currentEthPrice)
                {
                    var delta = currentEthPrice - convertedEthPrice;
                    var percentageGain = delta / currentEthPrice;
                    return ("buyWithBtc", (double)percentageGain);
                }

                // what are the results if buying this token with ETH and selling with BTC
                // sequence of events with example NEO
                // 1. Buy NEO with ETH, at the lowest ask price
                // 2. Sell NEO for BTC, at the highest bid price
                // 3. Sell the BTC for ETH, at the highest BTC-ETH bid price
                // The above only makes sense if the pairing converts at a higher price than what you can sell BTC-ETH at
                var buyingEthPrice = eth.LowestAsk;
                var sellingBtcPrice = btc.HighestBid;

                convertedEthPrice = sellingBtcPrice / buyingEthPrice;

                if (convertedEthPrice > currentEthPrice)
                {
                    var delta = convertedEthPrice - currentEthPrice;
                    var percentageGain = delta / currentEthPrice;
                    return ("buyWithEth", (double)percentageGain);
                }

                return ("none", -1d);
            }
        }

        /*decimal CalculateOpportunity(ExchangePriceInfo priceInfo)
        {

        }*/
    }
}
