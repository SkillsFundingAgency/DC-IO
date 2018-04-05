using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using ESFA.DC.IO.AzureStorage.Config.Interfaces;
using ESFA.DC.IO.Interfaces;
using ESFA.DC.Logging;
using ESFA.DC.Logging.Interfaces;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace ESFA.DC.IO.AzureStorage
{
    public sealed class AzureStorageKeyValuePersistenceService : IKeyValuePersistenceService
    {
        private readonly IAzureStorageKeyValuePersistenceServiceConfig _keyValuePersistenceServiceConfig;

        private readonly ILogger _logger;

        private CloudBlobContainer _cloudBlobContainer;

        public AzureStorageKeyValuePersistenceService(IAzureStorageKeyValuePersistenceServiceConfig keyValuePersistenceServiceConfig, ILogger logger)
        {
            _keyValuePersistenceServiceConfig = keyValuePersistenceServiceConfig;
            _logger = logger;
        }

        public async Task SaveAsync(string key, string value)
        {
            using (new TimedLogger(_logger, "Storage Set"))
            {
                await InitConnectionAsync();
                CloudBlockBlob blob = _cloudBlobContainer.GetBlockBlobReference(key);
                await blob.UploadTextAsync(value);
            }
        }

        public async Task<string> GetAsync(string key)
        {
            string value;
            using (new TimedLogger(_logger, "Storage Get"))
            {
                await InitConnectionAsync();
                CloudBlockBlob blob = _cloudBlobContainer.GetBlockBlobReference(key);
                if (blob == null)
                {
                    throw new KeyNotFoundException($"Key '{key}' was not found in the Azure Storage");
                }

                value = await blob.DownloadTextAsync();
            }

            return value;
        }

        public async Task RemoveAsync(string key)
        {
            using (new TimedLogger(_logger, "Storage Remove"))
            {
                await InitConnectionAsync();
                CloudBlockBlob blob = _cloudBlobContainer.GetBlockBlobReference(key);
                if (blob == null)
                {
                    throw new KeyNotFoundException($"Key '{key}' was not found in the Azure Storage");
                }

                await blob.DeleteAsync();
            }
        }

        private async Task InitConnectionAsync()
        {
            if (_cloudBlobContainer != null)
            {
                return;
            }

            CloudStorageAccount cloudStorageAccount = CloudStorageAccount.Parse(_keyValuePersistenceServiceConfig.ConnectionString);
            CloudBlobClient cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();
            _cloudBlobContainer = cloudBlobClient.GetContainerReference("persistence");
            await _cloudBlobContainer.CreateIfNotExistsAsync();
            ServicePoint tableServicePoint = ServicePointManager.FindServicePoint(cloudStorageAccount.TableEndpoint);
            tableServicePoint.ConnectionLimit = 1000;
        }
    }
}
