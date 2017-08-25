using NPoloniex.API;
using System.Collections.Generic;

namespace StinkBidder.Monitor
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
