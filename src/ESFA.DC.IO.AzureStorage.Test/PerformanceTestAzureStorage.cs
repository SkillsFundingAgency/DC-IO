using System.Threading.Tasks;
using ESFA.DC.Logging.Interfaces;
using Microsoft.Xunit.Performance;
using Moq;
using Xunit;

namespace ESFA.DC.IO.AzureStorage.Test
{
    public sealed class PerformanceTestAzureStorage : IClassFixture<TestFixture>
    {
        private readonly TestFixture _testFixture;

        public PerformanceTestAzureStorage(TestFixture testFixture)
        {
            _testFixture = testFixture;
        }

        [Benchmark]
        public async Task TestAzureStorage()
        {
            var loggerMock = new Mock<ILogger>();

            foreach (var iteration in Benchmark.Iterations)
            {
                using (iteration.StartMeasurement())
                {
                    var service = new AzureStorageKeyValuePersistenceService(_testFixture.Config, loggerMock.Object);
                    await service.SaveAsync("1_2_3", "Test Data");
                    await service.RemoveAsync("1_2_3");
                    await service.RemoveAsync("1_2_3");
                }
            }
        }
    }
}
