using System;
using System.Threading;

namespace TradeMonitor.WebJob.Poloniex
{
    class Program
    {
        static void Main(string[] args)
        {
            ManualResetEvent quitEvent = new ManualResetEvent(false);

            var listener = new Listener();
            listener.Start().Wait();
            Console.WriteLine("Poloniex trade listener started");
            quitEvent.WaitOne();
        }
    }
}