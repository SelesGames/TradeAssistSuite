using Owin;

namespace TradeAssist.Realtime
{
    public class WebAppStartup
    {
        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR();
        }
    }
}