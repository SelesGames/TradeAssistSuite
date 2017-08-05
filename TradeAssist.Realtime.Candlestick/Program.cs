using System.Linq;
using System.Threading;

namespace TradeAssist.Realtime.Candlestick
{
    class Program
    {
        static void Main(string[] args)
        {
            int port = int.TryParse(args?.FirstOrDefault(), out int p) ? p : 1943;

            ManualResetEvent quitEvent = new ManualResetEvent(false);

            using (var serviceStartup = new ServiceStartup())
            {
                serviceStartup.Startup($"http://localhost:{port}").Wait();
                quitEvent.WaitOne();
            }
        }
    }
}