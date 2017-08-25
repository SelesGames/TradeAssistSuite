using System;
using System.Collections.Generic;
using System.Text;

namespace TradeAssist.DocumentStorage
{
    public class TradeAssistUser
    {
        public string id { get; set; }
        public BittrexCredentials Bittrex { get; set; }
    }

    public class BittrexCredentials
    {
        public string ApiKey { get; set; }
        public string Secret { get; set; }
    }
}