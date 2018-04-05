using System;
using System.Configuration;
using ESFA.DC.IO.AzureCosmos.Config.Interfaces;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Moq;

namespace ESFA.DC.IO.AzureCosmos.Test
{
    public sealed class TestFixture : IDisposable
    {
        public const string DatabaseName = "Persistence";

        public const string DocumentCollectionName = "PersistenceCollection";

        public TestFixture()
        {
            string uri = ConfigurationManager.AppSettings["Uri"];
            string primaryKey = ConfigurationManager.AppSettings["PrimaryKey"];

            var mock = new Mock<IAzureCosmosKeyValuePersistenceServiceConfig>();
            mock.SetupGet(x => x.EndpointUrl).Returns(uri);
            mock.SetupGet(x => x.AuthKeyOrResourceToken).Returns(primaryKey);
            Config = mock.Object;

            Client = new DocumentClient(new Uri(uri), primaryKey);
            Client.CreateDatabaseIfNotExistsAsync(new Database { Id = DatabaseName }).GetAwaiter().GetResult();
            Client.CreateDocumentCollectionIfNotExistsAsync(UriFactory.CreateDatabaseUri(DatabaseName), new DocumentCollection { Id = DocumentCollectionName }).GetAwaiter().GetResult();
        }

        public IAzureCosmosKeyValuePersistenceServiceConfig Config { get; }

        public DocumentClient Client { get; }

        public void Dispose()
        {
            Client.Dispose();
        }
    }
}
