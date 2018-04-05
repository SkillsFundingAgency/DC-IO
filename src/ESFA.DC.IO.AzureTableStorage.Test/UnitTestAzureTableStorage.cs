using System.Threading.Tasks;
using ESFA.DC.IO.AzureTableStorage.Model;
using ESFA.DC.Logging.Interfaces;
using FluentAssertions;
using Microsoft.WindowsAzure.Storage.Table;
using Moq;
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
            var loggerMock = new Mock<ILogger>();

            var service = new AzureTableStorageKeyValuePersistenceService(_testFixture.Config, loggerMock.Object);
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
            var loggerMock = new Mock<ILogger>();

            DataExchange dataExchange = new DataExchange(Key, Value);
            TableOperation tableOperation = TableOperation.InsertOrReplace(dataExchange);
            await _testFixture.Container.ExecuteAsync(tableOperation);

            var service = new AzureTableStorageKeyValuePersistenceService(_testFixture.Config, loggerMock.Object);
            string ret = await service.GetAsync(Key);

            ret.Should().Be(Value);
        }

        [Fact]
        public async Task TestRemove()
        {
            const string Key = "1_2_3_Remove";
            const string Value = "Test Data";
            var loggerMock = new Mock<ILogger>();

            DataExchange dataExchange = new DataExchange(Key, Value);
            TableOperation tableOperation = TableOperation.InsertOrReplace(dataExchange);
            await _testFixture.Container.ExecuteAsync(tableOperation);

            var service = new AzureTableStorageKeyValuePersistenceService(_testFixture.Config, loggerMock.Object);
            await service.RemoveAsync(Key);

            TableOperation retrieveOperation = TableOperation.Retrieve<DataExchange>("A", Key);
            TableResult retrievedResult = await _testFixture.Container.ExecuteAsync(retrieveOperation);
            Assert.Null(retrievedResult.Result);
        }
    }
}
