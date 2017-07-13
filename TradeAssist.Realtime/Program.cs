using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Infrastructure;
using Microsoft.Owin.Hosting;
using NPoloniex.API.Push;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeAssist.Realtime
{


    class Program
    {

        static void Main(string[] args)
        {
            using (var serviceStartup = new ServiceStartup())
            {
                serviceStartup.Startup("http://localhost:1942").Wait();
                Console.ReadLine();
            }
        }
    }
}
