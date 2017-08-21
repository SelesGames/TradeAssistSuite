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

        public event EventHandler<UpdateSummaryState> OnUpdateSummaryStateTick;
        public event EventHandler<UpdateExchangeState> OnUpdateExchangeStateTick;

        public async Task Initialize()
        {
            var hubConnection = new HubConnection(BittrexConnectionString);
            //hubConnection.AsObservable().Subscribe(new observer());


            hubProxy = hubConnection.CreateHubProxy("coreHub");


            //hubProxy.On<UpdateSummaryState>("updateSummaryState", OnUpdateSummaryState);
            hubProxy.On<UpdateExchangeState>("updateExchangeState", OnUpdateExchangeState);

            /*hubProxy.On<dynamic>("updateFloodState", OnData);
            hubProxy.On<dynamic>("updateOrderState", OnData);*/

            await hubConnection.Start();       
        }

        public async Task<bool> SubscribeToTicker()
        {
            var result = await hubProxy.Invoke<dynamic>("SubscribeToExchangeDeltas", "null");

            if (result == true)
                Console.WriteLine("Subscribed to all Bittrex ticker deltas");
            else
                Console.WriteLine("Failed to subscribe");

            return result;
        }

        public async Task<bool> SubscribeToTicker(string currencyPair)
        {
            var result = await hubProxy.Invoke<dynamic>("SubscribeToExchangeDeltas", currencyPair);

            /*var orderState = await hubProxy.Invoke<dynamic>("QueryOrderState", currencyPair);
            Console.WriteLine(orderState ?? "");

            var exchangeState = await hubProxy.Invoke<dynamic>("QueryExchangeState", currencyPair);
            Console.WriteLine(exchangeState ?? "");*/

            if (result == true)
                Console.WriteLine($"Subscribed to {currencyPair} deltas");
            else
                Console.WriteLine("Failed to subscribe");

            return result;
        }

        public async Task SubscribeToFlood()
        {
            await hubProxy.Invoke("subscribeToFloodDeltas");
        }

        void OnData(dynamic d)
        {
            Console.WriteLine(d);
        }

        void OnUpdateSummaryState(UpdateSummaryState p)
        {
            if (p != null)
            {
                OnUpdateSummaryStateTick?.Invoke(this, p);
            }
        }

        void OnUpdateExchangeState(UpdateExchangeState p)
        {
            if (p != null)
            {
                OnUpdateExchangeStateTick?.Invoke(this, p);
            }
        }
    }
}
