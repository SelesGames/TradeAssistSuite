using System.Threading;

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
                //Console.ReadLine();
                //var waithandle = new AutoResetEvent(false);
                //waithandle.WaitOne();
                quitEvent.WaitOne();
            }
        }
    }
}