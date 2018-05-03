using System.IO;
using System.Threading.Tasks;
using ESFA.DC.IO.FileSystem.Config.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace ESFA.DC.IO.FileSystem.Test
{
    public class UnitTestFileSystem : IClassFixture<TestFixture>
    {
        private readonly TestFixture _testFixture;

        public UnitTestFileSystem(TestFixture testFixture)
        {
            _testFixture = testFixture;
        }

        [Fact]
        public void TestPath()
        {
            var configMock = new Mock<IFileSystemKeyValuePersistenceServiceConfig>();
            configMock.SetupGet(x => x.Directory).Returns(@"C:\SomePath\");

            var service = new FileSystemKeyValuePersistenceService(configMock.Object);
            service.GetFilename("1_2_3").Should().Be(@"C:\SomePath\1_2_3.dat");
        }

        [Fact]
        public async Task TestSet()
        {
            const string expectedFile = @"Storage\1_2_3.dat";

            var service = new FileSystemKeyValuePersistenceService(_testFixture.Config);
            await service.SaveAsync("1_2_3", "Test Data");

            File.Exists(expectedFile).Should().BeTrue();
            File.ReadAllText(expectedFile).Should().Be("Test Data");
        }

        [Fact]
        public async Task TestGet()
        {
            const string expectedFile = @"Storage\1_2_3.dat";

            File.WriteAllText(expectedFile, "Test Data");

            var service = new FileSystemKeyValuePersistenceService(_testFixture.Config);
            string ret = await service.GetAsync("1_2_3");

            ret.Should().Be("Test Data");
        }

        [Fact]
        public async Task TestRemove()
        {
            const string expectedFile = @"Storage\1_2_3.dat";

            File.WriteAllText(expectedFile, "Test Data");

            var service = new FileSystemKeyValuePersistenceService(_testFixture.Config);
            await service.RemoveAsync("1_2_3");

            File.Exists(expectedFile).Should().BeFalse();
        }

        [Fact]
        public async Task TestContains()
        {
            const string expectedFile = @"Storage\1_2_3.dat";

            File.WriteAllText(expectedFile, "Test Data");

            var service = new FileSystemKeyValuePersistenceService(_testFixture.Config);
            bool ret = await service.ContainsAsync("1_2_3");

            ret.Should().Be(true);
        }
    }
}
