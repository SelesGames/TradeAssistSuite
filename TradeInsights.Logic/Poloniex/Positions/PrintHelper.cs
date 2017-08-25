using Newtonsoft.Json;
using System;

namespace TradeInsights.Logic.Poloniex.Positions
{
    public static class PrintHelper
    {
        public static void PrettyPrint(this OpenPosition openPosition)
        {
            var output = new
            {
                realized = openPosition.RealizedProfit,
                unrealized = openPosition.UnrealizedProfit,
                total = openPosition.CurrentTotalProfit,
            };
            var jsonString = JsonConvert.SerializeObject(output);
            Console.WriteLine(jsonString);
        }
    }
}