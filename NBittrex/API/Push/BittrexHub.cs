using Microsoft.AspNet.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NBittrex
{
    public class BittrexHub : IDisposable
    {
        const string BittrexConnectionString = "https://socket.bittrex.com/signalr";
        readonly HubConnection hubConnection;
        readonly IHubProxy hubProxy;
        readonly List<IDisposable> subscriptions = new List<IDisposable>();

        public event EventHandler<UpdateSummaryState> OnUpdateSummaryStateTick;
        public event EventHandler<UpdateExchangeState> OnUpdateExchangeStateTick;

        public BittrexHub()
        {
            hubConnection = new HubConnection(BittrexConnectionString);
            hubProxy = hubConnection.CreateHubProxy("coreHub");
        }

        public async Task Connect()
        {
            if (hubConnection.State != ConnectionState.Disconnected)
                return;

            subscriptions.Add(
                hubProxy.On<UpdateSummaryState>("updateSummaryState", OnUpdateSummaryState));
            subscriptions.Add(
                hubProxy.On<UpdateExchangeState>("updateExchangeState", OnUpdateExchangeState));

            await hubConnection.Start();       
        }

        /// <summary>
        /// Subscribes to all exchange delta events
        /// </summary>
        /// <returns>Boolean value indicating subscription success</returns>
        public async Task<bool> SubscribeToTicker()
        {
            var result = await hubProxy.Invoke<dynamic>("SubscribeToExchangeDeltas", "null");

            if (result == true)
                Console.WriteLine("Subscribed to all Bittrex ticker deltas");
            else
                Console.WriteLine("Failed to subscribe to all currency Pair ticker deltas");

            return result;
        }

         /// <summary>
        /// Subscribes to  exchange delta events for a particular currencyPair
        /// </summary>
        /// <returns>Boolean value indicating subscription success</returns>
        public async Task<bool> SubscribeToTicker(string currencyPair)
        {
            var result = await hubProxy.Invoke<dynamic>("SubscribeToExchangeDeltas", currencyPair);

            if (result == true)
                Console.WriteLine($"Subscribed to {currencyPair} deltas");
            else
                Console.WriteLine($"Failed to subscribe to currencypair: {currencyPair}");

            return result;
        }

        public async Task<bool> SubscribeToFlood()
        {
            var result = await hubProxy.Invoke<dynamic>("subscribeToFloodDeltas");

            if (result == true)
                Console.WriteLine($"Subscribed to flood deltas");
            else
                Console.WriteLine("Failed to subscribe to flood deltas");

            return result;
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

        public void Dispose()
        {
            hubConnection.Dispose();
            foreach (var sub in subscriptions)
                sub.Dispose();
        }
    }
}


/*Unused example code:
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

    void OnData(dynamic d)
        {
            Console.WriteLine(d);
        }

hubConnection.AsObservable().Subscribe(new observer());

hubProxy.On<dynamic>("updateFloodState", OnData);
hubProxy.On<dynamic>("updateOrderState", OnData);

var orderState = await hubProxy.Invoke<dynamic>("QueryOrderState", currencyPair);
Console.WriteLine(orderState ?? "");

var exchangeState = await hubProxy.Invoke<dynamic>("QueryExchangeState", currencyPair);
Console.WriteLine(exchangeState ?? "");*/
