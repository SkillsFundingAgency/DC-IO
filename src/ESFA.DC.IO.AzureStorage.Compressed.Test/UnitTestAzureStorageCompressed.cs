using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.WindowsAzure.Storage.Blob;
using Xunit;

namespace ESFA.DC.IO.AzureStorage.Compressed.Test
{
    public sealed class UnitTestAzureStorageCompressed : IClassFixture<TestFixture>
    {
        private readonly TestFixture _testFixture;

        public UnitTestAzureStorageCompressed(TestFixture testFixture)
        {
            _testFixture = testFixture;
        }

        [Fact]
        public async Task TestSet()
        {
            const string Key = "1_2_3_Set";
            const string Value = "Test Data";

            var service = new AzureStorageCompressedKeyValuePersistenceService(_testFixture.Config);
            await service.SaveAsync(Key, Value);

            CloudBlockBlob blob = _testFixture.Container.GetBlockBlobReference(BuildKey(Key));
            blob.Exists().Should().BeTrue();

            blob.FetchAttributes();
            long fileByteLength = blob.Properties.Length;
            byte[] data = new byte[fileByteLength];

            byte[] val = await ZipAsync(Value);
            await blob.DownloadToByteArrayAsync(data, 0);
            data.Should().BeSubsetOf(val);
        }

        [Fact]
        public async Task TestGet()
        {
            const string Key = "1_2_3_Get";
            const string Value = "Test Data";

            CloudBlockBlob blob = _testFixture.Container.GetBlockBlobReference(BuildKey(Key));
            byte[] val = await ZipAsync(Value);
            await blob.UploadFromByteArrayAsync(val, 0, val.Length);

            var service = new AzureStorageCompressedKeyValuePersistenceService(_testFixture.Config);
            string ret = await service.GetAsync(Key);

            ret.Should().Be(Value);
        }

        [Fact]
        public async Task TestSetGet()
        {
            const string Key = "1_2_3_SetGet";
            const string Value = "Test Data";

            var service = new AzureStorageCompressedKeyValuePersistenceService(_testFixture.Config);
            await service.SaveAsync(Key, Value);
            string ret = await service.GetAsync(Key);

            ret.Should().Be(Value);
        }

        [Fact]
        public async Task TestSetGetAlgorithm()
        {
            const string Value = "Test Data";

            byte[] data = _testFixture.Config.ValueEncoding.GetBytes(Value);

            using (MemoryStream comp = new MemoryStream())
            {
                using (GZipStream gzip = new GZipStream(comp, _testFixture.Config.CompressionLevel))
                {
                    await gzip.WriteAsync(data, 0, data.Length);
                }

                data = comp.ToArray();
            }

            using (MemoryStream comp = new MemoryStream(data))
            {
                using (MemoryStream decomp = new MemoryStream())
                {
                    using (GZipStream gzip = new GZipStream(comp, CompressionMode.Decompress))
                    {
                        gzip.CopyTo(decomp);
                    }

                    data = decomp.ToArray();
                }
            }

            _testFixture.Config.ValueEncoding.GetString(data).Should().Be(Value);
        }

        [Fact]
        public async Task TestRemove_Positive()
        {
            const string Key = "1_2_3_Remove";
            const string Value = "Test Data";

            CloudBlockBlob blob = _testFixture.Container.GetBlockBlobReference(BuildKey(Key));
            await blob.UploadTextAsync(Value);

            var service = new AzureStorageCompressedKeyValuePersistenceService(_testFixture.Config);
            await service.RemoveAsync(Key);

            blob.Exists().Should().BeFalse();
        }

        [Fact]
        public async Task TestRemove_Negative()
        {
            const string Key = "1_2_3_Remove";

            var service = new AzureStorageCompressedKeyValuePersistenceService(_testFixture.Config);
            await Assert.ThrowsAsync<KeyNotFoundException>(() => service.RemoveAsync(Key));
        }

        [Fact]
        public async Task TestContains_Positive()
        {
            const string Key = "1_2_3_Get";
            const string Value = "Test Data";

            CloudBlockBlob blob = _testFixture.Container.GetBlockBlobReference(BuildKey(Key));
            await blob.UploadTextAsync(Value);

            var service = new AzureStorageCompressedKeyValuePersistenceService(_testFixture.Config);
            bool ret = await service.ContainsAsync(Key);

            ret.Should().Be(true);
        }

        [Fact]
        public async Task TestContains_Negative()
        {
            const string Key = "1_2_3_Negative";

            var service = new AzureStorageCompressedKeyValuePersistenceService(_testFixture.Config);
            bool ret = await service.ContainsAsync(Key);

            ret.Should().Be(false);
        }

        private static string BuildKey(string key)
        {
            return key.Replace('_', '/');
        }

        private async Task<byte[]> ZipAsync(string str)
        {
            var data = _testFixture.Config.ValueEncoding.GetBytes(str);

            using (MemoryStream comp = new MemoryStream())
            {
                using (GZipStream gzip = new GZipStream(comp, _testFixture.Config.CompressionLevel))
                {
                    await gzip.WriteAsync(data, 0, data.Length);
                }

                data = comp.ToArray();
            }

            return data;
        }
    }
}
