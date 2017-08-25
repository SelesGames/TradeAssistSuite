using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TradeAssist.DocumentStorage
{
    public class TADocumentClient
    {
        static DocumentClient client;

        internal DocumentClient Client => client;

        //private static List<ValueTuple<string, string>> DatabaseCollectionPairs = new List<ValueTuple<string, string>>
        private static List<(string, string)> DatabaseCollectionPairs = new List<(string, string)>
        {
            //("ToDoList", "Items"),
            ("User", "Users"),
            ("TradeAssist", "StinkBids")
        };

        internal TADocumentClient(string endpointUrl, string primaryKey)
        {
            client = new DocumentClient(new Uri(endpointUrl), primaryKey, new ConnectionPolicy { EnableEndpointDiscovery = false });

            Users = new TypedDocumentClient<TradeAssistUser>(this,
                DatabaseCollectionPairs[0].Item1,
                DatabaseCollectionPairs[0].Item2);

            StinkBids = new TypedDocumentClient<StinkBid>(this,
                DatabaseCollectionPairs[1].Item1,
                DatabaseCollectionPairs[1].Item2);

            UsersDynamic = new DynamicDocumentClient(this,
                DatabaseCollectionPairs[0].Item1,
                DatabaseCollectionPairs[0].Item2);

            StinkBidsDynamic = new DynamicDocumentClient(this,
                DatabaseCollectionPairs[1].Item1,
                DatabaseCollectionPairs[1].Item2);
        }

        public TypedDocumentClient<StinkBid> StinkBids { get; }
        public TypedDocumentClient<TradeAssistUser> Users { get; }
        public DynamicDocumentClient StinkBidsDynamic { get; }
        public DynamicDocumentClient UsersDynamic { get; }



        #region Initialization

        public static async Task InitializeDatabaseAndCollectionsAsync()
        {
            var distinctDatabases = DatabaseCollectionPairs.Select(db => db.Item1).Distinct();
            //await Task.WhenAll(distinctDatabases.Select(CreateDatabaseIfNotExistsAsync));
            //await Task.WhenAll(DatabaseCollectionPairs.Select(o => CreateCollectionIfNotExistsAsync(o.Item1, o.Item2)));

            foreach (var item in distinctDatabases)
            {
                await CreateDatabaseIfNotExistsAsync(item);
            }

            foreach (var (db, coll) in DatabaseCollectionPairs)
            {
                await CreateCollectionIfNotExistsAsync(db, coll);
            }
        }

        static async Task CreateDatabaseIfNotExistsAsync(string databaseId)
        {
            // alternative
            await client.CreateDatabaseIfNotExistsAsync(new Database { Id = databaseId });

            /*try
            {
                await client.ReadDatabaseAsync(UriFactory.CreateDatabaseUri(databaseId));
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    await client.CreateDatabaseAsync(new Database { Id = databaseId });
                }
                else
                {
                    throw;
                }
            }*/
        }

        static async Task CreateCollectionIfNotExistsAsync(string databaseId, string collectionId)
        {
            // -----------------------------------------
            // assumes database has already been created
            // -----------------------------------------

            // alternative to below
            await client.CreateDocumentCollectionIfNotExistsAsync(UriFactory.CreateDatabaseUri(databaseId), new DocumentCollection { Id = collectionId });

            /*try
            {
                await client.ReadDocumentCollectionAsync(UriFactory.CreateDocumentCollectionUri(DatabaseId, CollectionId));
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    await client.CreateDocumentCollectionAsync(
                        UriFactory.CreateDatabaseUri(DatabaseId),
                        new DocumentCollection { Id = CollectionId },
                        new RequestOptions { OfferThroughput = 1000 });
                }
                else
                {
                    throw;
                }
            }*/
        }

        #endregion create database and collections
    }
}