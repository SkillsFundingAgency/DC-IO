using System.Threading.Tasks;
using ESFA.DC.IO.AzureTableStorage.Model;
using FluentAssertions;
using Microsoft.WindowsAzure.Storage.Table;
using Xunit;

namespace ESFA.DC.IO.AzureTableStorage.Test
{
    public sealed class UnitTestAzureTableStorage : IClassFixture<TestFixture>
    {
        private readonly TestFixture _testFixture;

        public UnitTestAzureTableStorage(TestFixture testFixture)
        {
            _testFixture = testFixture;
        }

        [Fact]
        public async Task TestSet()
        {
            const string Key = "1_2_3_Set";
            const string Value = "Test Data";

            var service = new AzureTableStorageKeyValuePersistenceService(_testFixture.Config);
            await service.SaveAsync(Key, Value);

            TableOperation retrieveOperation = TableOperation.Retrieve<DataExchange>("A", Key);
            TableResult retrievedResult = await _testFixture.Container.ExecuteAsync(retrieveOperation);
            DataExchange deleteEntity = retrievedResult.Result as DataExchange;
            Assert.NotNull(deleteEntity);
            deleteEntity.Value.Should().Be(Value);
        }

        [Fact]
        public async Task TestGet()
        {
            const string Key = "1_2_3_Get";
            const string Value = "Test Data";

            DataExchange dataExchange = new DataExchange(Key, Value);
            TableOperation tableOperation = TableOperation.InsertOrReplace(dataExchange);
            await _testFixture.Container.ExecuteAsync(tableOperation);

            var service = new AzureTableStorageKeyValuePersistenceService(_testFixture.Config);
            string ret = await service.GetAsync(Key);

            ret.Should().Be(Value);
        }

        [Fact]
        public async Task TestRemove()
        {
            const string Key = "1_2_3_Remove";
            const string Value = "Test Data";

            DataExchange dataExchange = new DataExchange(Key, Value);
            TableOperation tableOperation = TableOperation.InsertOrReplace(dataExchange);
            await _testFixture.Container.ExecuteAsync(tableOperation);

            var service = new AzureTableStorageKeyValuePersistenceService(_testFixture.Config);
            await service.RemoveAsync(Key);

            TableOperation retrieveOperation = TableOperation.Retrieve<DataExchange>("A", Key);
            TableResult retrievedResult = await _testFixture.Container.ExecuteAsync(retrieveOperation);
            Assert.Null(retrievedResult.Result);
        }

        [Fact]
        public async Task TestContains()
        {
            const string Key = "1_2_3_Get";
            const string Value = "Test Data";

            DataExchange dataExchange = new DataExchange(Key, Value);
            TableOperation tableOperation = TableOperation.InsertOrReplace(dataExchange);
            await _testFixture.Container.ExecuteAsync(tableOperation);

            var service = new AzureTableStorageKeyValuePersistenceService(_testFixture.Config);
            bool ret = await service.ContainsAsync(Key);

            ret.Should().Be(true);
        }
    }
}
