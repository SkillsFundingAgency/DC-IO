using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.IO.AzureCosmos.Config.Interfaces;
using ESFA.DC.IO.AzureCosmos.Model;
using ESFA.DC.IO.Interfaces;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace ESFA.DC.IO.AzureCosmos
{
    public sealed class AzureCosmosKeyValuePersistenceService : IKeyValuePersistenceService, IDisposable
    {
        private const string DatabaseName = "Persistence";

        private const string DocumentCollectionName = "PersistenceCollection";

        private readonly IAzureCosmosKeyValuePersistenceServiceConfig _keyValuePersistenceServiceConfig;

        private readonly Dictionary<string, Uri> _uriCache = new Dictionary<string, Uri>();

        private readonly Uri _uriDocumentCollection = UriFactory.CreateDocumentCollectionUri(DatabaseName, DocumentCollectionName);

        private readonly SemaphoreSlim _initLock = new SemaphoreSlim(1, 1);

        private DocumentClient _client;

        public AzureCosmosKeyValuePersistenceService(IAzureCosmosKeyValuePersistenceServiceConfig keyValuePersistenceServiceConfig)
        {
            _keyValuePersistenceServiceConfig = keyValuePersistenceServiceConfig;
        }

        public async Task SaveAsync(string key, string value)
        {
            await InitConnection();
            DataExchange dataExchange = new DataExchange(key, value);
            await _client.UpsertDocumentAsync(_uriDocumentCollection, dataExchange);
        }

        public async Task<string> GetAsync(string key)
        {
            await InitConnection();
            IQueryable<DataExchange> query = _client.CreateDocumentQuery<DataExchange>(
                _uriDocumentCollection,
                new SqlQuerySpec("Select * From PersistenceCollection pc Where pc.id = @id", new SqlParameterCollection { new SqlParameter("@id", key) }));
            var enumerable = query.ToArray();
            if (!enumerable.Any())
            {
                throw new KeyNotFoundException($"Key '{key}' was not found in the store");
            }

            return enumerable.Single().Value;
        }

        public async Task RemoveAsync(string key)
        {
            await InitConnection();
            await _client.DeleteDocumentAsync(GetDocUri(key));
        }

        public async Task<bool> ContainsAsync(string key)
        {
            await InitConnection();
            IOrderedQueryable<Document> query = _client.CreateDocumentQuery(_uriDocumentCollection, new FeedOptions { MaxItemCount = 1 });
            return query.Where(x => x.Id == key).Select(x => x.Id).AsEnumerable().Any();
        }

        public void Dispose()
        {
            _client?.Dispose();
        }

        private async Task InitConnection()
        {
            await _initLock.WaitAsync();

            try
            {
                if (_client != null)
                {
                    return;
                }

                ServicePoint tableServicePoint =
                    ServicePointManager.FindServicePoint(new Uri(_keyValuePersistenceServiceConfig.EndpointUrl));
                tableServicePoint.ConnectionLimit = 1000;
                _client = new DocumentClient(
                    new Uri(_keyValuePersistenceServiceConfig.EndpointUrl),
                    _keyValuePersistenceServiceConfig.AuthKeyOrResourceToken,
                    new ConnectionPolicy { ConnectionMode = ConnectionMode.Direct, ConnectionProtocol = Protocol.Tcp });
                await _client.OpenAsync();
                await _client.CreateDatabaseIfNotExistsAsync(new Database { Id = DatabaseName });
                await _client.CreateDocumentCollectionIfNotExistsAsync(
                    UriFactory.CreateDatabaseUri(DatabaseName),
                    new DocumentCollection { Id = DocumentCollectionName });
            }
            finally
            {
                _initLock.Release();
            }
        }

        private Uri GetDocUri(string key)
        {
            if (!_uriCache.TryGetValue(key, out var value))
            {
                value = UriFactory.CreateDocumentUri(
                    DatabaseName,
                    DocumentCollectionName,
                    key);
                _uriCache[key] = value;
            }

            return value;
        }
    }
}
