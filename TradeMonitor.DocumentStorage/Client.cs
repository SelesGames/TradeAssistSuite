using DocumentStorageHelpers;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace TradeMonitor.DocumentStorage
{
    public class Client
    {
        // in-memory cache of seen collectionNames
        SortedSet<string> collectionNames = new SortedSet<string>();

        readonly DocumentClient innerClient;

        const string tradeDatabase = "TradeMonitor";

        public static Client Current { get; } = new Client();
        private Client()
        {
            innerClient = DbClientFactory.Current.GetDocumentClient();
        }

        public async Task Warmup()
        {
            await innerClient.OpenAsync();
        }

        public async Task<(bool result, HttpStatusCode statusCode)> SaveTrade(Trade trade)
        {
            var collectionName = getCollectionName();

            await CheckCollectionName(collectionName);

            Exception saveException;
            HttpStatusCode responseStatusCode = HttpStatusCode.SeeOther;

            try
            {
                var response = await innerClient.CreateDocumentAsync(
                    documentCollectionUri: getTradeUri(),
                    document: trade,
                    disableAutomaticIdGeneration: false);

                responseStatusCode = response?.StatusCode ?? responseStatusCode;

                return (response?.Resource?.Id == trade.Id, responseStatusCode);
            }
            catch(Exception ex)
            {
                saveException = ex;
            }

            if (saveException != null)
                handleSaveException();

            // if we made it this far, the save failed
            return (false, responseStatusCode);


            string getCollectionName()
            {
                var exchange = trade.Exchange.ToLower();
                var market = trade.Market.ToLower();

                return $"{exchange}:{market}"; // uses an URN schema
            }

            Uri getTradeUri()
            {          
                return UriFactory.CreateDocumentCollectionUri(tradeDatabase, collectionName);
            }

            void handleSaveException()
            {
                // detect if it is a ConflictException - meaning there was already a document with that ID present
                try
                {
                    var dyn = (dynamic)saveException;
                    var statusCode = dyn.StatusCode as HttpStatusCode?;
                    if (statusCode.HasValue && statusCode.Value == HttpStatusCode.Conflict)
                    {
                        responseStatusCode = statusCode.Value;
                        onDocumentWithSameIdAlreadyExists();
                    }
                }
                catch { }
            }

            void onDocumentWithSameIdAlreadyExists()
            {
                Console.WriteLine($"Document with id: {trade.Id} already exists");
            }
        }

        async Task CheckCollectionName(string collectionName)
        {
            if (!collectionNames.Contains(collectionName))
            {
                if (collectionNames.Add(collectionName))
                {
                    var response = await innerClient.CreateDocumentCollectionIfNotExistsAsync(
                        UriFactory.CreateDatabaseUri(tradeDatabase), 
                        new DocumentCollection { Id = collectionName });

                    if (response.Resource.Id == collectionName)
                        Console.WriteLine($"Created collection: {collectionName}");
                }
            }
        }

        public string CreateNamespacedTradeId(string id)
        {
            return $"trade:{id}";
        }
    }
}