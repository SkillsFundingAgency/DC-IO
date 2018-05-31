using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using ESFA.DC.IO.AzureStorage.Config.Interfaces;
using ESFA.DC.IO.Interfaces;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace ESFA.DC.IO.AzureStorage
{
    public sealed class AzureStorageKeyValuePersistenceService : IKeyValuePersistenceService
    {
        private readonly IAzureStorageKeyValuePersistenceServiceConfig _keyValuePersistenceServiceConfig;

        private CloudBlobContainer _cloudBlobContainer;

        public AzureStorageKeyValuePersistenceService(IAzureStorageKeyValuePersistenceServiceConfig keyValuePersistenceServiceConfig)
        {
            _keyValuePersistenceServiceConfig = keyValuePersistenceServiceConfig;
        }

        public async Task SaveAsync(string key, string value)
        {
            key = BuildKey(key);
            await InitConnectionAsync();
            CloudBlockBlob blob = _cloudBlobContainer.GetBlockBlobReference(key);
            await blob.UploadTextAsync(value);
        }

        public async Task<string> GetAsync(string key)
        {
            key = BuildKey(key);
            await InitConnectionAsync();
            CloudBlockBlob blob = _cloudBlobContainer.GetBlockBlobReference(key);
            if (blob == null)
            {
                throw new KeyNotFoundException($"Key '{key}' was not found in the store");
            }

            return await blob.DownloadTextAsync();
        }

        public async Task RemoveAsync(string key)
        {
            key = BuildKey(key);
            await InitConnectionAsync();
            CloudBlockBlob blob = _cloudBlobContainer.GetBlockBlobReference(key);
            if (blob == null)
            {
                throw new KeyNotFoundException($"Key '{key}' was not found in the store");
            }

            await blob.DeleteAsync();
        }

        public async Task<bool> ContainsAsync(string key)
        {
            key = BuildKey(key);
            await InitConnectionAsync();
            CloudBlockBlob blob = _cloudBlobContainer.GetBlockBlobReference(key);
            return blob != null;
        }

        private static string BuildKey(string key)
        {
            return key.Replace('_', '/');
        }

        private async Task InitConnectionAsync()
        {
            if (_cloudBlobContainer != null)
            {
                return;
            }

            CloudStorageAccount cloudStorageAccount = CloudStorageAccount.Parse(_keyValuePersistenceServiceConfig.ConnectionString);
            CloudBlobClient cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();
            await ConnectToContainer(cloudBlobClient);
            UnlockConnectionLimit(cloudStorageAccount);
        }

        private void UnlockConnectionLimit(CloudStorageAccount cloudStorageAccount)
        {
            ServicePoint tableServicePoint = ServicePointManager.FindServicePoint(cloudStorageAccount.TableEndpoint);
            tableServicePoint.ConnectionLimit = 1000;
        }

        private async Task ConnectToContainer(CloudBlobClient cloudBlobClient)
        {
            string containerName = _keyValuePersistenceServiceConfig.ContainerName;
            if (string.IsNullOrEmpty(containerName))
            {
                containerName = "persistence";
            }

            _cloudBlobContainer = cloudBlobClient.GetContainerReference(containerName);
            await _cloudBlobContainer.CreateIfNotExistsAsync();
        }
    }
}
