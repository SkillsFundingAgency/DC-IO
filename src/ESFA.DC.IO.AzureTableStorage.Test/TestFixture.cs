using System;
using System.Configuration;
using ESFA.DC.IO.AzureTableStorage.Config.Interfaces;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Moq;

namespace ESFA.DC.IO.AzureTableStorage.Test
{
    public sealed class TestFixture : IDisposable
    {
        public IAzureTableStorageKeyValuePersistenceServiceConfig Config { get; }

        public CloudTable Container { get; }

        public TestFixture()
        {
            string connectionString = ConfigurationManager.AppSettings["ConnectionStringAzureStorage"];

            var mock = new Mock<IAzureTableStorageKeyValuePersistenceServiceConfig>();
            mock.SetupGet(x => x.ConnectionString).Returns(connectionString);
            Config = mock.Object;

            CloudStorageAccount cloudStorageAccount = CloudStorageAccount.Parse(connectionString);
            CloudTableClient cloudBlobClient = cloudStorageAccount.CreateCloudTableClient();
            Container = cloudBlobClient.GetTableReference("persistence");
            Container.CreateIfNotExists();
        }

        public void Dispose()
        {
            Container.DeleteIfExists();
        }
    }
}
