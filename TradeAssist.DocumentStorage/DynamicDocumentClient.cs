using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace TradeAssist.DocumentStorage
{
    public class DynamicDocumentClient
    {
        readonly string databaseId;
        readonly string collectionId;
        readonly DocumentClient client;

        internal DynamicDocumentClient(TADocumentClient client, string databaseId, string collectionId)
        {
            this.client = client.Client;
            this.databaseId = databaseId;
            this.collectionId = collectionId;
        }

        public async Task<dynamic> Get(string id)
        {
            try
            {
                dynamic item = await client.ReadDocumentAsync<dynamic>(UriFactory.CreateDocumentUri(databaseId, collectionId, id));
                return item.Document;
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return default(dynamic);
                }
                else
                {
                    throw;
                }
            }
        }

        public async Task<IEnumerable<dynamic>> GetMany(
            Expression<Func<dynamic, bool>> predicate)
        {
            IDocumentQuery<dynamic> query = client.CreateDocumentQuery(
                    UriFactory.CreateDocumentCollectionUri(databaseId, collectionId),
                    new FeedOptions { MaxItemCount = -1 })
                .Where(predicate)
                .AsDocumentQuery();

            List<dynamic> results = new List<dynamic>();
            while (query.HasMoreResults)
            {
                results.AddRange(await query.ExecuteNextAsync());
            }

            return results;
        }

        public async Task<Document> Create(dynamic item)
        {
            return await client.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(databaseId, collectionId), item);
        }

        public async Task<Document> Update(string id, dynamic item)
        {
            return await client.ReplaceDocumentAsync(UriFactory.CreateDocumentUri(databaseId, collectionId, id), item);
        }

        public async Task Delete(string id)
        {
            await client.DeleteDocumentAsync(UriFactory.CreateDocumentUri(databaseId, collectionId, id));
        }
    }
}