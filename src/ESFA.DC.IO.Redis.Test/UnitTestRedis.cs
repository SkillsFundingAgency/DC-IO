using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace ESFA.DC.IO.Redis.Test
{
    public class UnitTestRedis : IClassFixture<TestFixture>
    {
        private readonly TestFixture _testFixture;

        public UnitTestRedis(TestFixture testFixture)
        {
            _testFixture = testFixture;
        }

        [Fact]
        public async Task TestSet()
        {
            const string key = "1_2_3_Set";
            const string expectedValue = "Test Data";

            var service = new RedisKeyValuePersistenceService(_testFixture.Config);
            await service.SaveAsync(key, expectedValue);

            _testFixture.Database.KeyExists(key).Should().BeTrue();
            _testFixture.Database.StringGet(key).Should().Be(expectedValue);
        }

        [Fact]
        public async Task TestGet()
        {
            const string key = "1_2_3_Set";
            const string expectedValue = "Test Data";

            _testFixture.Database.StringSet(key, expectedValue);

            var service = new RedisKeyValuePersistenceService(_testFixture.Config);
            string ret = await service.GetAsync(key);

            ret.Should().Be(expectedValue);
        }

        [Fact]
        public async Task TestRemove()
        {
            const string key = "1_2_3_Set";
            const string expectedValue = "Test Data";

            _testFixture.Database.StringSet(key, expectedValue);

            var service = new RedisKeyValuePersistenceService(_testFixture.Config);
            bool res = await service.ContainsAsync(key);

            res.Should().BeTrue();
        }
    }
}
