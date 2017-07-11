using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeAssist.Realtime
{
    [HubName("ticker")]
    public class TickerHub : Hub
    {
        public void Subscribe(string currencyPair)
        {
            Groups.Add(Context.ConnectionId, currencyPair);
        }

        public void Unsubscribe(string currencyPair)
        {
            Groups.Remove(Context.ConnectionId, currencyPair);
        }
    }

    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR();
        }
    }
}
