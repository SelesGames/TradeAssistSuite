using Microsoft.AspNet.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NBittrex
{
        /*    {
          "MarketName": "ETH-RLC",
          "High": 0.002136,
          "Low": 0.00158949,
          "Volume": 87798.54800921,
          "Last": 0.00187165,
          "BaseVolume": 163.66531362,
          "TimeStamp": "2017-07-07T07:14:29.06",
          "Bid": 0.00181001,
          "Ask": 0.00188957,
          "OpenBuyOrders": 49,
          "OpenSellOrders": 118,
          "PrevDay": 0.00209301,
          "Created": "2017-04-30T18:41:22.823"
        },*/

    public class Payload
    {
        public long Nounce { get; set; }
        public List<Delta> Deltas { get; set; }
    }

    public class Delta
    {
        public string MarketName { get; set; }
        public decimal High { get; set; }
        public decimal Low { get; set; }
        public decimal Volume { get; set; }
        public decimal Last { get; set; }
        public decimal BaseVolume { get; set; }
        public decimal Bid { get; set; }
        public decimal Ask { get; set; }
        public decimal PrevDay { get; set; }
    }

   /* class observer : IObserver<string>
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
    }*/

    public class BittrexHub
    {
        string BittrexConnectionString = "https://socket.bittrex.com/signalr";
        IHubProxy hubProxy;

        public event EventHandler<Payload> OnPayloadTick;

        public async Task Initialize()
        {
            var hubConnection = new HubConnection(BittrexConnectionString);
            //hubConnection.AsObservable().Subscribe(new observer());
            hubProxy = hubConnection.CreateHubProxy("coreHub");
            hubProxy.On<Payload>("updateSummaryState", OnPayload);
            //hubProxy.On("updateSummaryState", OnData);

            //stockTickerHubProxy.On("SubscribeToExchangeDeltas", dynamic d => OnData()

            await hubConnection.Start();       
        }

        public async Task SubscribeToTicker(string tickerPair)
        {       
            //var result = await hubProxy.Invoke<dynamic>("QueryExchangeState", tickerPair);
            var result = await hubProxy.Invoke<dynamic>("SubscribeToExchangeDeltas", tickerPair);
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

        void OnPayload(Payload p)
        {
            if (p != null)
            {
                OnPayloadTick?.Invoke(this, p);
            }
            /*var relevantOne = p.Deltas.SingleOrDefault(o => o.MarketName == "BTC-XRP");

            if (relevantOne != null)
            {
                var output = Newtonsoft.Json.JsonConvert.SerializeObject(relevantOne);

                Console.WriteLine(output);
            }*/
        }      
    }
}
