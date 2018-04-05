using System.Threading.Tasks;
using ESFA.DC.Logging.Interfaces;
using FluentAssertions;
using Microsoft.WindowsAzure.Storage.Blob;
using Moq;
using Xunit;

namespace ESFA.DC.IO.AzureStorage.Test
{
    public class UnitTestAzureStorage : IClassFixture<TestFixture>
    {
        private readonly TestFixture _testFixture;

        public UnitTestAzureStorage(TestFixture testFixture)
        {
            _testFixture = testFixture;
        }

        [Fact]
        public async Task TestSet()
        {
            const string Key = "1_2_3_Set";
            const string Value = "Test Data";
            var loggerMock = new Mock<ILogger>();

            var service = new AzureStorageKeyValuePersistenceService(_testFixture.Config, loggerMock.Object);
            await service.SaveAsync(Key, Value);

            CloudBlockBlob blob = _testFixture.Container.GetBlockBlobReference(Key);
            blob.Exists().Should().BeTrue();
            (await blob.DownloadTextAsync()).Should().Be(Value);
        }

        [Fact]
        public async Task TestGet()
        {
            const string Key = "1_2_3_Get";
            const string Value = "Test Data";
            var loggerMock = new Mock<ILogger>();

            CloudBlockBlob blob = _testFixture.Container.GetBlockBlobReference(Key);
            await blob.UploadTextAsync(Value);

            var service = new AzureStorageKeyValuePersistenceService(_testFixture.Config, loggerMock.Object);
            string ret = await service.GetAsync(Key);

            ret.Should().Be(Value);
        }

        [Fact]
        public async Task TestRemove()
        {
            const string Key = "1_2_3_Remove";
            const string Value = "Test Data";
            var loggerMock = new Mock<ILogger>();

            CloudBlockBlob blob = _testFixture.Container.GetBlockBlobReference(Key);
            await blob.UploadTextAsync(Value);

            var service = new AzureStorageKeyValuePersistenceService(_testFixture.Config, loggerMock.Object);
            await service.RemoveAsync(Key);

            blob.Exists().Should().BeFalse();
        }
    }
}
