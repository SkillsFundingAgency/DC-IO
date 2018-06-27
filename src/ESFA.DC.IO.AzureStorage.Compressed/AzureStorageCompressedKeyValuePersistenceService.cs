﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.IO.AzureStorage.Compressed.Config.Interfaces;
using ESFA.DC.IO.Interfaces;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace ESFA.DC.IO.AzureStorage.Compressed
{
    public sealed class AzureStorageCompressedKeyValuePersistenceService : IKeyValuePersistenceService
    {
        private readonly IAzureStorageCompressedKeyValuePersistenceServiceConfig _keyValuePersistenceServiceConfig;

        private readonly SemaphoreSlim _initLock = new SemaphoreSlim(1, 1);

        private CloudBlobContainer _cloudBlobContainer;

        public AzureStorageCompressedKeyValuePersistenceService(
            IAzureStorageCompressedKeyValuePersistenceServiceConfig keyValuePersistenceServiceConfig)
        {
            _keyValuePersistenceServiceConfig = keyValuePersistenceServiceConfig;
        }

        public async Task SaveAsync(string key, string value)
        {
            key = BuildKey(key);
            await InitConnectionAsync();
            CloudBlockBlob blob = _cloudBlobContainer.GetBlockBlobReference(key);
            blob.Metadata.Add("compressed", bool.TrueString);
            byte[] data = _keyValuePersistenceServiceConfig.ValueEncoding.GetBytes(value);
            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (GZipStream gZipStream = new GZipStream(memoryStream, _keyValuePersistenceServiceConfig.CompressionLevel))
                {
                    await gZipStream.WriteAsync(data, 0, data.Length);
                }

                data = memoryStream.ToArray();
            }

            await blob.UploadFromByteArrayAsync(data, 0, data.Length);
        }

        public async Task<string> GetAsync(string key)
        {
            key = BuildKey(key);
            await InitConnectionAsync();
            CloudBlockBlob blockReference = _cloudBlobContainer.GetBlockBlobReference(key);
            var exists = await blockReference.ExistsAsync();
            if (!exists)
            {
                throw new KeyNotFoundException($"Key '{key}' was not found in the store");
            }

            await blockReference.FetchAttributesAsync();
            if (blockReference.Metadata.ContainsKey("compressed") && !Convert.ToBoolean(blockReference.Metadata["compressed"]))
            {
                throw new KeyNotFoundException($"Key '{key}' contains non-compressed content");
            }

            byte[] data;
            using (MemoryStream comp = new MemoryStream())
            {
                using (MemoryStream decomp = new MemoryStream())
                {
                    using (GZipStream gzip = new GZipStream(comp, CompressionMode.Decompress))
                    {
                        await blockReference.DownloadToStreamAsync(comp);
                        comp.Seek(0, SeekOrigin.Begin);
                        await gzip.CopyToAsync(decomp);
                    }

                    data = decomp.ToArray();
                }
            }

            return _keyValuePersistenceServiceConfig.ValueEncoding.GetString(data);
        }

        public async Task RemoveAsync(string key)
        {
            key = BuildKey(key);
            await InitConnectionAsync();
            var deleted = await _cloudBlobContainer.GetBlockBlobReference(key).DeleteIfExistsAsync();
            if (!deleted)
            {
                throw new KeyNotFoundException($"Key '{key}' was not found in the store");
            }
        }

        public async Task<bool> ContainsAsync(string key)
        {
            key = BuildKey(key);
            await InitConnectionAsync();
            return await _cloudBlobContainer.GetBlockBlobReference(key).ExistsAsync();
        }

        private static string BuildKey(string key)
        {
            return key.Replace('_', '/');
        }

        private async Task InitConnectionAsync()
        {
            await _initLock.WaitAsync();

            try
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
            finally
            {
                _initLock.Release();
            }
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
