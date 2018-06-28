using System.Linq;
using System.Threading.Tasks;
using ESFA.DC.IO.AzureCosmos.Model;
using FluentAssertions;
using Microsoft.Azure.Documents.Client;
using Xunit;

namespace ESFA.DC.IO.AzureCosmos.Test
{
    public class UnitTestAzureCosmos : IClassFixture<TestFixture>
    {
        private readonly TestFixture _testFixture;

        public UnitTestAzureCosmos(TestFixture testFixture)
        {
            _testFixture = testFixture;
        }

        [Fact]
        public async Task TestSet()
        {
            const string Key = "1_2_3_Set";
            const string Value = "Test Data";

            var service = new AzureCosmosKeyValuePersistenceService(_testFixture.Config);
            await service.SaveAsync(Key, Value);

            IQueryable<DataExchange> query = _testFixture.Client.CreateDocumentQuery<DataExchange>(
                    UriFactory.CreateDocumentCollectionUri(TestFixture.DatabaseName, TestFixture.DocumentCollectionName))
                .Where(k => k.Id == Key);
            query.AsEnumerable().Should().HaveCount(1);
            query.AsEnumerable().Single().Value.Should().Be(Value);
        }

        [Fact]
        public async Task TestGet()
        {
            const string Key = "1_2_3_Get";
            const string Value = "Test Data";

            DataExchange dataExchange = new DataExchange(Key, Value);
            await _testFixture.Client.UpsertDocumentAsync(UriFactory.CreateDocumentCollectionUri(TestFixture.DatabaseName, TestFixture.DocumentCollectionName), dataExchange);

            var service = new AzureCosmosKeyValuePersistenceService(_testFixture.Config);
            string ret = await service.GetAsync(Key);

            ret.Should().Be(Value);
        }

        [Fact]
        public async Task TestContains()
        {
            const string Key = "1_2_3_Get";
            const string Value = "Test Data";

            DataExchange dataExchange = new DataExchange(Key, Value);
            await _testFixture.Client.UpsertDocumentAsync(UriFactory.CreateDocumentCollectionUri(TestFixture.DatabaseName, TestFixture.DocumentCollectionName), dataExchange);

            var service = new AzureCosmosKeyValuePersistenceService(_testFixture.Config);
            bool ret = await service.ContainsAsync(Key);

            ret.Should().Be(true);
        }

        [Fact]
        public async Task TestRemove()
        {
            const string Key = "1_2_3_Remove";
            const string Value = "Test Data";

            DataExchange dataExchange = new DataExchange(Key, Value);
            await _testFixture.Client.UpsertDocumentAsync(UriFactory.CreateDocumentCollectionUri(TestFixture.DatabaseName, TestFixture.DocumentCollectionName), dataExchange);

            var service = new AzureCosmosKeyValuePersistenceService(_testFixture.Config);
            await service.RemoveAsync(Key);

            IQueryable<DataExchange> query = _testFixture.Client.CreateDocumentQuery<DataExchange>(
                    UriFactory.CreateDocumentCollectionUri(TestFixture.DatabaseName, TestFixture.DocumentCollectionName))
                .Where(k => k.Id == Key);
            query.AsEnumerable().Should().BeEmpty();
        }
    }
}
