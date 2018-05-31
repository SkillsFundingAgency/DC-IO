using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.WindowsAzure.Storage.Blob;
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

            var service = new AzureStorageKeyValuePersistenceService(_testFixture.Config);
            await service.SaveAsync(Key, Value);

            CloudBlockBlob blob = _testFixture.Container.GetBlockBlobReference(BuildKey(Key));
            blob.Exists().Should().BeTrue();
            (await blob.DownloadTextAsync()).Should().Be(Value);
        }

        [Fact]
        public async Task TestGet()
        {
            const string Key = "1_2_3_Get";
            const string Value = "Test Data";

            CloudBlockBlob blob = _testFixture.Container.GetBlockBlobReference(BuildKey(Key));
            await blob.UploadTextAsync(Value);

            var service = new AzureStorageKeyValuePersistenceService(_testFixture.Config);
            string ret = await service.GetAsync(Key);

            ret.Should().Be(Value);
        }

        [Fact]
        public async Task TestRemove_Positive()
        {
            const string Key = "1_2_3_Remove";
            const string Value = "Test Data";

            CloudBlockBlob blob = _testFixture.Container.GetBlockBlobReference(BuildKey(Key));
            await blob.UploadTextAsync(Value);

            var service = new AzureStorageKeyValuePersistenceService(_testFixture.Config);
            await service.RemoveAsync(Key);

            blob.Exists().Should().BeFalse();
        }

        [Fact]
        public async Task TestRemove_Negative()
        {
            const string Key = "1_2_3_Remove";

            var service = new AzureStorageKeyValuePersistenceService(_testFixture.Config);
            await Assert.ThrowsAsync<KeyNotFoundException>(() => service.RemoveAsync(Key));
        }
        
        [Fact]
        public async Task TestContains_Positive()
        {
            const string Key = "1_2_3_Get";
            const string Value = "Test Data";

            CloudBlockBlob blob = _testFixture.Container.GetBlockBlobReference(BuildKey(Key));
            await blob.UploadTextAsync(Value);

            var service = new AzureStorageKeyValuePersistenceService(_testFixture.Config);
            bool ret = await service.ContainsAsync(Key);

            ret.Should().Be(true);
        }
        
        [Fact]
        public async Task TestContains_Negative()
        {
            const string Key = "1_2_3_Negative";

            var service = new AzureStorageKeyValuePersistenceService(_testFixture.Config);
            bool ret = await service.ContainsAsync(Key);

            ret.Should().Be(false);
        }

        private static string BuildKey(string key)
        {
            return key.Replace('_', '/');
        }
    }
}
