using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.IO.AzureTableStorage.Config.Interfaces;
using ESFA.DC.IO.AzureTableStorage.Model;
using ESFA.DC.IO.Interfaces;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace ESFA.DC.IO.AzureTableStorage
{
    public sealed class AzureTableStorageKeyValuePersistenceService : IKeyValuePersistenceService
    {
        private readonly IAzureTableStorageKeyValuePersistenceServiceConfig _keyValuePersistenceServiceConfig;

        private readonly SemaphoreSlim _initLock = new SemaphoreSlim(1, 1);

        private CloudTable _cloudTableContainer;

        public AzureTableStorageKeyValuePersistenceService(IAzureTableStorageKeyValuePersistenceServiceConfig keyValuePersistenceServiceConfig)
        {
            _keyValuePersistenceServiceConfig = keyValuePersistenceServiceConfig;
        }

        public async Task SaveAsync(string key, string value, CancellationToken cancellationToken = default(CancellationToken))
        {
            await InitConnectionAsync(cancellationToken);

            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            DataExchange dataExchange = new DataExchange(key, value);
            TableOperation tableOperation = TableOperation.InsertOrReplace(dataExchange);
            await _cloudTableContainer.ExecuteAsync(tableOperation, null, null, cancellationToken);
        }

        public async Task<string> GetAsync(string key, CancellationToken cancellationToken = default(CancellationToken))
        {
            TableKey tableKey = new TableKey(key);
            await InitConnectionAsync(cancellationToken);

            if (cancellationToken.IsCancellationRequested)
            {
                return null;
            }

            TableOperation retrieveOperation = TableOperation.Retrieve<DataExchange>(tableKey.JobId.ToString(), key);
            TableResult retrievedResult = await _cloudTableContainer.ExecuteAsync(retrieveOperation, null, null, cancellationToken);

            if (cancellationToken.IsCancellationRequested)
            {
                return null;
            }

            DataExchange deleteEntity = retrievedResult.Result as DataExchange;
            if (deleteEntity == null)
            {
                throw new KeyNotFoundException($"Key '{key}' was not found in the store");
            }

            return deleteEntity.Value;
        }

        public async Task RemoveAsync(string key, CancellationToken cancellationToken = default(CancellationToken))
        {
            TableKey tableKey = new TableKey(key);
            await InitConnectionAsync(cancellationToken);

            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            TableOperation retrieveOperation = TableOperation.Retrieve<DataExchange>(tableKey.JobId.ToString(), key);
            TableResult retrievedResult = await _cloudTableContainer.ExecuteAsync(retrieveOperation, null, null, cancellationToken);

            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            DataExchange deleteEntity = retrievedResult.Result as DataExchange;
            if (deleteEntity == null)
            {
                throw new KeyNotFoundException($"Key '{key}' was not found in the store");
            }

            TableOperation deleteOperation = TableOperation.Delete(deleteEntity);
            await _cloudTableContainer.ExecuteAsync(deleteOperation, null, null, cancellationToken);
        }

        public async Task<bool> ContainsAsync(string key, CancellationToken cancellationToken = default(CancellationToken))
        {
            TableKey tableKey = new TableKey(key);
            await InitConnectionAsync(cancellationToken);

            if (cancellationToken.IsCancellationRequested)
            {
                return false;
            }

            TableOperation retrieveOperation = TableOperation.Retrieve<DataExchange>(tableKey.JobId.ToString(), key);
            TableResult retrievedResult = await _cloudTableContainer.ExecuteAsync(retrieveOperation, null, null, cancellationToken);
            return retrievedResult.Result is DataExchange;
        }

        private async Task InitConnectionAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            await _initLock.WaitAsync(cancellationToken);

            try
            {
                if (_cloudTableContainer != null)
                {
                    return;
                }

                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                CloudStorageAccount cloudStorageAccount = CloudStorageAccount.Parse(_keyValuePersistenceServiceConfig.ConnectionString);
                CloudTableClient cloudTableClient = cloudStorageAccount.CreateCloudTableClient();
                _cloudTableContainer = cloudTableClient.GetTableReference("persistence");
                await _cloudTableContainer.CreateIfNotExistsAsync(null, null, cancellationToken);

                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                ServicePoint tableServicePoint = ServicePointManager.FindServicePoint(cloudStorageAccount.TableEndpoint);
                tableServicePoint.ConnectionLimit = 1000;
            }
            finally
            {
                _initLock.Release();
            }
        }
    }
}
