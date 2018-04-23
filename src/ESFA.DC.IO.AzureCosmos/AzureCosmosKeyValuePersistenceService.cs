using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using ESFA.DC.IO.AzureCosmos.Config.Interfaces;
using ESFA.DC.IO.AzureCosmos.Model;
using ESFA.DC.IO.Interfaces;
using ESFA.DC.Logging;
using ESFA.DC.Logging.Interfaces;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace ESFA.DC.IO.AzureCosmos
{
    public sealed class AzureCosmosKeyValuePersistenceService : IKeyValuePersistenceService, IDisposable
    {
        private const string DatabaseName = "Persistence";

        private const string DocumentCollectionName = "PersistenceCollection";

        private readonly IAzureCosmosKeyValuePersistenceServiceConfig _keyValuePersistenceServiceConfig;

        private readonly ILogger _logger;

        private DocumentClient _client;

        private Dictionary<string, Uri> _uriCache;

        private Uri _uriDocumentCollection;

        public AzureCosmosKeyValuePersistenceService(IAzureCosmosKeyValuePersistenceServiceConfig keyValuePersistenceServiceConfig, ILogger logger)
        {
            _keyValuePersistenceServiceConfig = keyValuePersistenceServiceConfig;
            _logger = logger;
        }

        public async Task SaveAsync(string key, string value)
        {
            using (new TimedLogger(_logger, "Cosmos Set"))
            {
                await InitConnection();
                DataExchange dataExchange = new DataExchange(key, value);
                await _client.UpsertDocumentAsync(_uriDocumentCollection, dataExchange);
            }
        }

        public async Task<string> GetAsync(string key)
        {
            using (new TimedLogger(_logger, "Cosmos Get"))
            {
                await InitConnection();
                IQueryable<DataExchange> query = _client.CreateDocumentQuery<DataExchange>(
                    _uriDocumentCollection,
                    new SqlQuerySpec("Select * From PersistenceCollection pc Where pc.id = @id", new SqlParameterCollection { new SqlParameter("@id", key) }));
                return query.AsEnumerable().Single().Value;
            }
        }

        public async Task RemoveAsync(string key)
        {
            using (new TimedLogger(_logger, "Cosmos Remove"))
            {
                await InitConnection();
                await _client.DeleteDocumentAsync(GetDocUri(key));
            }
        }

        public void Dispose()
        {
            _client?.Dispose();
            _logger?.Dispose();
        }

        private async Task InitConnection()
        {
            if (_client != null)
            {
                return;
            }

            ServicePoint tableServicePoint = ServicePointManager.FindServicePoint(new Uri(_keyValuePersistenceServiceConfig.EndpointUrl));
            tableServicePoint.ConnectionLimit = 1000;
            _client = new DocumentClient(
                new Uri(_keyValuePersistenceServiceConfig.EndpointUrl),
                _keyValuePersistenceServiceConfig.AuthKeyOrResourceToken,
                new ConnectionPolicy { ConnectionMode = ConnectionMode.Direct, ConnectionProtocol = Protocol.Tcp });
            await _client.OpenAsync();
            await _client.CreateDatabaseIfNotExistsAsync(new Database { Id = DatabaseName });
            await _client.CreateDocumentCollectionIfNotExistsAsync(UriFactory.CreateDatabaseUri(DatabaseName), new DocumentCollection { Id = DocumentCollectionName });
            _uriCache = new Dictionary<string, Uri>();
            _uriDocumentCollection = UriFactory.CreateDocumentCollectionUri(DatabaseName, DocumentCollectionName);
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
