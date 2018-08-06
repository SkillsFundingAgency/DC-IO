using System;
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
    public sealed class AzureStorageCompressedKeyValuePersistenceService : IStreamableKeyValuePersistenceService
    {
        private readonly IAzureStorageCompressedKeyValuePersistenceServiceConfig _keyValuePersistenceServiceConfig;

        private readonly SemaphoreSlim _initLock = new SemaphoreSlim(1, 1);

        private CloudBlobContainer _cloudBlobContainer;

        public AzureStorageCompressedKeyValuePersistenceService(
            IAzureStorageCompressedKeyValuePersistenceServiceConfig keyValuePersistenceServiceConfig)
        {
            _keyValuePersistenceServiceConfig = keyValuePersistenceServiceConfig;
        }

        public async Task SaveAsync(string key, string value, CancellationToken cancellationToken = default(CancellationToken))
        {
            key = BuildKey(key);
            await InitConnectionAsync(cancellationToken);

            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            CloudBlockBlob blob = _cloudBlobContainer.GetBlockBlobReference(key);
            blob.Metadata.Add("compressed", bool.TrueString);
            byte[] data = _keyValuePersistenceServiceConfig.ValueEncoding.GetBytes(value);
            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (GZipStream gZipStream = new GZipStream(memoryStream, _keyValuePersistenceServiceConfig.CompressionLevel))
                {
                    await gZipStream.WriteAsync(data, 0, data.Length, cancellationToken);
                }

                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                data = memoryStream.ToArray();
            }

            await blob.UploadFromByteArrayAsync(data, 0, data.Length, null, null, null, cancellationToken);
        }

        public async Task<string> GetAsync(string key, CancellationToken cancellationToken = default(CancellationToken))
        {
            key = BuildKey(key);
            await InitConnectionAsync(cancellationToken);

            if (cancellationToken.IsCancellationRequested)
            {
                return null;
            }

            CloudBlockBlob blockReference = _cloudBlobContainer.GetBlockBlobReference(key);
            var exists = await blockReference.ExistsAsync(null, null, cancellationToken);
            if (!exists)
            {
                throw new KeyNotFoundException($"Key '{key}' was not found in the store");
            }

            if (cancellationToken.IsCancellationRequested)
            {
                return null;
            }

            if (!await CheckAttributes(key, blockReference, cancellationToken))
            {
                return null;
            }

            byte[] data;
            using (MemoryStream comp = new MemoryStream())
            {
                using (MemoryStream decomp = new MemoryStream())
                {
                    using (GZipStream gzip = new GZipStream(comp, CompressionMode.Decompress))
                    {
                        await blockReference.DownloadToStreamAsync(comp, null, null, null, cancellationToken);

                        if (cancellationToken.IsCancellationRequested)
                        {
                            return null;
                        }

                        comp.Seek(0, SeekOrigin.Begin);
                        await gzip.CopyToAsync(decomp, 81920, cancellationToken);

                        if (cancellationToken.IsCancellationRequested)
                        {
                            return null;
                        }
                    }

                    data = decomp.ToArray();
                }
            }

            return _keyValuePersistenceServiceConfig.ValueEncoding.GetString(data);
        }

        public async Task RemoveAsync(string key, CancellationToken cancellationToken = default(CancellationToken))
        {
            key = BuildKey(key);
            await InitConnectionAsync(cancellationToken);

            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            var deleted = await _cloudBlobContainer.GetBlockBlobReference(key).DeleteIfExistsAsync(DeleteSnapshotsOption.None, null, null, null, cancellationToken);
            if (!deleted)
            {
                throw new KeyNotFoundException($"Key '{key}' was not found in the store");
            }
        }

        public async Task<bool> ContainsAsync(string key, CancellationToken cancellationToken = default(CancellationToken))
        {
            key = BuildKey(key);
            await InitConnectionAsync(cancellationToken);

            if (cancellationToken.IsCancellationRequested)
            {
                return false;
            }

            return await _cloudBlobContainer.GetBlockBlobReference(key).ExistsAsync(null, null, cancellationToken);
        }

        public async Task SaveAsync(string key, Stream value, CancellationToken cancellationToken = default(CancellationToken))
        {
            key = BuildKey(key);
            await InitConnectionAsync(cancellationToken);

            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            CloudBlockBlob blob = _cloudBlobContainer.GetBlockBlobReference(key);
            blob.Metadata.Add("compressed", bool.TrueString);
            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (GZipStream gZipStream = new GZipStream(memoryStream, _keyValuePersistenceServiceConfig.CompressionLevel))
                {
                    value.Seek(0, SeekOrigin.Begin);
                    await value.CopyToAsync(gZipStream, 81920, cancellationToken);
                }

                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                memoryStream.Seek(0, SeekOrigin.Begin);

                await blob.UploadFromStreamAsync(memoryStream, null, null, null, cancellationToken);
            }
        }

        public async Task GetAsync(string key, Stream value, CancellationToken cancellationToken = default(CancellationToken))
        {
            key = BuildKey(key);
            await InitConnectionAsync(cancellationToken);

            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            CloudBlockBlob blockReference = _cloudBlobContainer.GetBlockBlobReference(key);
            var exists = await blockReference.ExistsAsync(null, null, cancellationToken);
            if (!exists)
            {
                throw new KeyNotFoundException($"Key '{key}' was not found in the store");
            }

            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            if (!await CheckAttributes(key, blockReference, cancellationToken))
            {
                return;
            }

            using (MemoryStream comp = new MemoryStream())
            {
                using (GZipStream gzip = new GZipStream(comp, CompressionMode.Decompress))
                {
                    await blockReference.DownloadToStreamAsync(comp, null, null, null, cancellationToken);

                    if (cancellationToken.IsCancellationRequested)
                    {
                        return;
                    }

                    comp.Seek(0, SeekOrigin.Begin);
                    value.Seek(0, SeekOrigin.Begin);
                    await gzip.CopyToAsync(value, 81920, cancellationToken);
                }
            }
        }

        private static string BuildKey(string key)
        {
            return key.Replace('_', '/');
        }

        private async Task InitConnectionAsync(CancellationToken cancellationToken)
        {
            await _initLock.WaitAsync(cancellationToken);

            try
            {
                if (_cloudBlobContainer != null)
                {
                    return;
                }

                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                CloudStorageAccount cloudStorageAccount = CloudStorageAccount.Parse(_keyValuePersistenceServiceConfig.ConnectionString);
                CloudBlobClient cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();
                await ConnectToContainer(cloudBlobClient, cancellationToken);

                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }

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

        private async Task ConnectToContainer(CloudBlobClient cloudBlobClient, CancellationToken cancellationToken)
        {
            string containerName = _keyValuePersistenceServiceConfig.ContainerName;
            if (string.IsNullOrEmpty(containerName))
            {
                containerName = "persistence";
            }

            _cloudBlobContainer = cloudBlobClient.GetContainerReference(containerName);
            await _cloudBlobContainer.CreateIfNotExistsAsync(BlobContainerPublicAccessType.Off, null, null, cancellationToken);
        }

        private async Task<bool> CheckAttributes(string key, CloudBlockBlob blockReference, CancellationToken cancellationToken)
        {
            await blockReference.FetchAttributesAsync(null, null, null, cancellationToken);

            if (cancellationToken.IsCancellationRequested)
            {
                return false;
            }

            if (blockReference.Metadata.ContainsKey("compressed") && !Convert.ToBoolean(blockReference.Metadata["compressed"]))
            {
                throw new KeyNotFoundException($"Key '{key}' contains compressed content");
            }

            return true;
        }
    }
}
