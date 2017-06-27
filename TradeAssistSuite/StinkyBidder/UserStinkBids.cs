using NPoloniex.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeAssist.Suite
{
    public class Users : List<User>
    {
    }

    public class User
    {
        public string PublicKey { get; set; }
        public string PrivateKey { get; set; }
        public UserStinkBids StinkBids { get; } = new UserStinkBids();
    }

    public class UserStinkBids
    {
        public Dictionary<CurrencyPair, StinkBid> StinkBids { get; } = new Dictionary<CurrencyPair, StinkBid>();
    }
}
