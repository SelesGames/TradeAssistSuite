using Newtonsoft.Json;
using Shared.Http;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace NBinance.API.Push
{
    /// <summary>
    /// Generic class for subscribing to websockets that pump json data
    /// with help from https://gist.github.com/xamlmonkey/4737291
    /// </summary>
    /// <typeparam name="T"></typeparam>
    class JsonWebSocketClient2<T> : IDisposable, IWebSocketMessageHandler
    {
        static readonly Encoding readEncoding = Encoding.UTF8;
        static readonly JsonSerializer jsonSerializer =
            new JsonSerializer { NullValueHandling = NullValueHandling.Ignore };

        readonly BaseWebSocketClient client;

        public IOnDataHandler<T> OnDataHandler { get; set; }

        public JsonWebSocketClient2(string uri)
        {
            client = new BaseWebSocketClient(uri)
            {
                ReconnectOnServerClose = true,
                TextMessageHandler = this
            };
        }

        public void OnMessageReceived(MemoryStream ms)
        {
            var message = DeserializeJsonToObject();
            OnDataHandler?.HandleData(message);

            T DeserializeJsonToObject()
            {
                using (var sr = new StreamReader(ms, readEncoding, false))
                using (var reader = new JsonTextReader(sr))
                {
                    return jsonSerializer.Deserialize<T>(reader);
                }
            }
        }

        public Task Connect()
        {
            return client.Connect();
        }

        public void Dispose()
        {
            client.Dispose();
        }
    }
}