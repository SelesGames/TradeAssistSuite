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
            var queryStringData = new Dictionary<string, string>();
            queryStringData.Add("clientProtocol", "1.2");
            queryStringData.Add("connectionData", "%5B%7B%22name%22%3A%22corehub%22%7D%5D");
            queryStringData.Add("_", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString());

            hubConnection = new HubConnection(BittrexConnectionString, queryStringData);

            hubConnection.Headers.Add(
                "User-Agent", 
                "Mozilla/5.0 (Linux; Android 6.0; Nexus 5 Build/MRA58N) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/60.0.3112.113 Mobile Safari/537.36");
            //hubConnection.Headers.Add("Content-Type", "application/x-www-form-urlencoded; charset=UTF-8");
            hubConnection.Headers.Add("Connection", "keep-alive");
            hubConnection.Headers.Add("DNT", "1");
            hubConnection.Headers.Add("Origin", "https://bittrex.com");
            hubConnection.Headers.Add("Referer", "https://bittrex.com/Market/Index?MarketName=BTC-cvc");

            hubConnection.Headers.Add(
                "Cookie",
                "__cfduid=d120ac696405bc894bccd38861d2d13df1494188742; cf_clearance=13a453310832c34251a59bab994005fbd10de3ee-1505016634-10800; .AspNet.ApplicationCookie=t_MRD0AOWJx9MD32Ww21nmXLcu5K11vBnhlgmf6SU2Tp-g0NzwIIRK9ddvrBcJD_2Mkyy2hkE3FFJD1WHlZh_zUenlFN28Al5CT3SqtU9ePFiespHZRzRXbsQKD8ijSzShAoJsTNRboqasrwG4XnnjdC0JQL8WgtV3pU2G-fsrG7_8K88zU5Uw23VSrhzeiv04OPsOLakQNtLGq7KNPuUjer3_NvrQ6vh4_nlReyxB32AlzPGLgNkQQ_oMux6dPvqhyL_vyUAcbgdfzzwZeHyJFcjxtGXq7BraBpcI6kK4StHHBF62Ho95MO16TBWNTr8VNmASzRhGZj-8eqr7EyBnMMvRH_kAuVl7kl-ZwOvsCjikg0L-g6SLWtQIswIqyZ-KCz0AQXL7QwVkJ3ux60uYvIl7Bz1OORLe9g2KQKyPlUdFkXK4Fa8ms6PHf-3BNJ-0ZupJS7p0xx9pS3_V_pvv8TpCo");

            hubConnection.Protocol = new Version("1.2");
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

            try
            {
                await hubConnection.Start();
                Console.WriteLine("bittrex hub connected");
            }
            catch(Exception ex)
            {
                Console.WriteLine($"bittrex could not connect: {ex}");
            }
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
