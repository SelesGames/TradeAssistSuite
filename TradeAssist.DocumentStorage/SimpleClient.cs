using Microsoft.Azure.Documents.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;

namespace TradeAssist.DocumentStorage
{
    internal class SimpleClient
    {
        public DocumentClient Client { get; }

        internal SimpleClient(
            string endpointUrl,
            string primaryKey,
            bool enableEndpointDiscovery = false,
            TimeSpan? requestTimeout = null,
            bool useTcpInsteadOfHttps = false,
            bool useStreamingInsteadOfBuffering = false
            )
        {
            var connectionPolicy = new ConnectionPolicy
            {
                EnableEndpointDiscovery = false,
                ConnectionMode = ConnectionMode.Direct,
                ConnectionProtocol = useTcpInsteadOfHttps ? Protocol.Tcp : Protocol.Https,
                //MaxConnectionLimit = 
                MediaReadMode = useStreamingInsteadOfBuffering ? MediaReadMode.Streamed : MediaReadMode.Buffered,
            };
            if (requestTimeout.HasValue)
                connectionPolicy.RequestTimeout = requestTimeout.Value;

            var serializerSettings = new JsonSerializerSettings
            {
                //DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate,
                Formatting = Formatting.None,
                MissingMemberHandling = MissingMemberHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore,
                //TypeNameHandling = TypeNameHandling.None,
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
            };
            Client = new DocumentClient(
                serviceEndpoint: new Uri(endpointUrl),
                authKeyOrResourceToken: primaryKey,
                connectionPolicy: connectionPolicy, 
                serializerSettings: serializerSettings);
        }
    }
}