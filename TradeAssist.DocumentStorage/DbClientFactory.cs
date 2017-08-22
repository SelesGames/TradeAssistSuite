using System;

namespace TradeAssist.DocumentStorage
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

        public TADocumentClient Get()
        {
            if (string.IsNullOrEmpty(endpointUrl) || string.IsNullOrEmpty(primaryKey))
                throw new InvalidOperationException("You must call SetCredentials before attempting Document DB access");

            return new TADocumentClient(endpointUrl, primaryKey);
        }
    }
}