using System;
using System.Collections.Generic;
using System.Configuration;
using ESFA.DC.IO.AzureStorage.Config.Interfaces;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Moq;

namespace ESFA.DC.IO.AzureStorage.Test
{
    public sealed class TestFixture : IDisposable
    {
        public IAzureStorageKeyValuePersistenceServiceConfig Config { get; }

        public CloudBlobContainer Container { get; }

        public TestFixture()
        {
            string connectionString = ConfigurationManager.AppSettings["ConnectionStringAzureTableStorage"];

            var mock = new Mock<IAzureStorageKeyValuePersistenceServiceConfig>();
            mock.SetupGet(x => x.ConnectionString).Returns(connectionString);
            Config = mock.Object;

            CloudStorageAccount cloudStorageAccount = CloudStorageAccount.Parse(connectionString);
            CloudBlobClient cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();
            Container = cloudBlobClient.GetContainerReference("persistence");
            Container.CreateIfNotExists();

            Cleanup(null);
        }

        public void Dispose()
        {
            Cleanup(null);
        }

        private void Cleanup(IListBlobItem blobItem)
        {
            if (blobItem == null)
            {
                IEnumerable<IListBlobItem> list = Container.ListBlobs();
                foreach (IListBlobItem listBlobItem in list)
                {
                    Cleanup(listBlobItem);
                }

                return;
            }

            if (blobItem is CloudBlobDirectory directory)
            {
                IEnumerable<IListBlobItem> list = directory.ListBlobs();
                foreach (IListBlobItem listBlobItem in list)
                {
                    Cleanup(listBlobItem);
                }
            }
            else
            {
                ((CloudBlockBlob)blobItem).Delete();
            }
        }
    }
}
