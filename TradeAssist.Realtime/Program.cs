using System.Threading;
using TradeAssist.Realtime.SignalR;

namespace TradeAssist.Realtime
{
    class Program
    {
        static void Main(string[] args)
        {
            ManualResetEvent quitEvent = new ManualResetEvent(false);

            using (var serviceStartup = new ServiceStartup())
            {
                serviceStartup.Startup("http://localhost:1942").Wait();
                quitEvent.WaitOne();
            }
        }
    }
}