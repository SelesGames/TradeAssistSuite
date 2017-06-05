using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace TradeAssistSuite
{
    public static class PrintHelper
    {
        public static void PrettyPrint(OpenPosition openPosition)
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