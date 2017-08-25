using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeAssistSuite
{
    public class Environment
    {
        public static Environment Current = new Environment();
        private Environment() { }

        public string UserId { get; set; }
        public string BittrexApiKey { get; set; }
        public string BittrexSecret { get; set; }
    }
}