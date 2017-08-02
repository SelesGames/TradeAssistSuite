using Microsoft.Azure.EventHubs;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;

namespace TradeAssist.Web
{
    public class EventHubRouter
    {
        public static EventHubRouter Current { get; } = new EventHubRouter();
        private EventHubRouter() { }

        string connectionString;
        string eventHubName;
        EventHubClient client;
        JsonReadWriter jsonRW = new JsonReadWriter();

        public void Initialize(string connectionString, string eventHubName)
        {
            this.connectionString = connectionString;
            this.eventHubName = eventHubName;

            var connectionStringBuilder = new EventHubsConnectionStringBuilder(connectionString)
            {
                EntityPath = eventHubName
            };

            var fullConnectionString = connectionStringBuilder.ToString();

            client = EventHubClient.CreateFromConnectionString(fullConnectionString);
        }

        public async Task SendAsJson<T>(T obj)
        {
            var payload = jsonRW.Serialize(obj);
            var eventData = new EventData(payload);
            await client.SendAsync(eventData);
        }
    }




    class JsonReadWriter
    {
        static readonly JsonSerializer jsonSerializer =
    new JsonSerializer { NullValueHandling = NullValueHandling.Ignore };

        public byte[] Serialize(object obj)
        {
            Exception deserializeException = null;
            byte[] bytes = null;

            try
            {
                using (var ms = new MemoryStream())
                using (var sw = new StreamWriter(ms))
                using (var jw = new JsonTextWriter(sw))
                {
                    jsonSerializer.Serialize(jw, obj);
                    bytes = ms.ToArray();
                }
            }
            catch (Exception ex)
            {
                deserializeException = ex;
            }

            if (deserializeException != null)
            {
                throw new JsonWriterException($"error writing {obj.ToString()}", deserializeException);
            }

            return bytes;
        }
    }
}