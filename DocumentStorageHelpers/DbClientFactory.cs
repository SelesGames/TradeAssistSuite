using Microsoft.Azure.Documents.Client;

namespace DocumentStorageHelpers
{
    public class DbClientFactory
    {
        public static DbClientFactory Current { get; } = new DbClientFactory();
        private DbClientFactory() { }

        string endpointUrl;
        string primaryKey;

        public void SetCredentials(string endpointUrl, string primaryKey)
        {
            this.endpointUrl = endpointUrl;
            this.primaryKey = primaryKey;
        }

        public DocumentClient GetDocumentClient()
        {
            return new SimpleClient(endpointUrl, primaryKey).Client;
        }
    }
}