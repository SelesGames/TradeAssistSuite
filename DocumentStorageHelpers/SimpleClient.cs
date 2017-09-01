using Microsoft.Azure.Documents.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;

namespace DocumentStorageHelpers
{
    class SimpleClient
    {
        public DocumentClient Client { get; }

        // performance tips: https://docs.microsoft.com/en-us/azure/cosmos-db/performance-tips
        internal SimpleClient(
            string endpointUrl,
            string primaryKey,
            bool enableEndpointDiscovery = false,
            TimeSpan? requestTimeout = null,
            //bool useTcpInsteadOfHttps = true,
            bool useStreamingInsteadOfBuffering = false
            )
        {
            var connectionPolicy = new ConnectionPolicy
            {
                EnableEndpointDiscovery = false,
                ConnectionMode = ConnectionMode.Direct,
                ConnectionProtocol = Protocol.Tcp,
                //ConnectionProtocol = useTcpInsteadOfHttps ? Protocol.Tcp : Protocol.Https,
                MaxConnectionLimit = 100,
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