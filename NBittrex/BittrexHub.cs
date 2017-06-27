using Microsoft.AspNet.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NBittrex
{
    class observer : IObserver<string>
    {
        public void OnCompleted()
        {
            throw new NotImplementedException();
        }

        public void OnError(Exception error)
        {
            throw new NotImplementedException();
        }

        public void OnNext(string value)
        {
            Console.WriteLine(value);
        }
    }

    public class BittrexHub
    {
        string BittrexConnectionString = "https://socket.bittrex.com/signalr";
        IHubProxy hubProxy;

        public async Task Initialize()
        {
            var hubConnection = new HubConnection(BittrexConnectionString);
            hubConnection.AsObservable().Subscribe(new observer());
            hubProxy = hubConnection.CreateHubProxy("coreHub");

            //stockTickerHubProxy.On("SubscribeToExchangeDeltas", dynamic d => OnData()

            await hubConnection.Start();
        }

        public async Task SubscribeToTicker(string tickerPair)
        {       
            var result = await hubProxy.Invoke<dynamic>("QueryExchangeState", tickerPair);
            //var result = await hubProxy.Invoke<dynamic>("SubscribeToExchangeDeltas", tickerPair);
            Console.WriteLine(result);
           /* if (result == true)
            {
                hubProxy.On($"data-query-exchange-{tickerPair}", OnData);
            }*/
        }

        void OnData(dynamic d)
        {
            Console.WriteLine(d);
        }
        
    }
}
