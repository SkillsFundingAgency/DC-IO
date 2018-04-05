using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using ESFA.DC.IO.SqlServer.Model;
using ESFA.DC.Logging.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace ESFA.DC.IO.SqlServer.Test
{
    public class UnitTestSql : IClassFixture<TestFixture>
    {
        private readonly TestFixture _testFixture;

        public UnitTestSql(TestFixture testFixture)
        {
            _testFixture = testFixture;
        }

        [Fact]
        public async Task TestSet()
        {
            const string key = "1_2_1";
            const string expectedValue = "Test Data";
            var loggerMock = new Mock<ILogger>();

            var service = new SqlServerKeyValuePersistenceService(_testFixture.Config, loggerMock.Object);
            await service.SaveAsync(key, expectedValue);

            DataExchange de = (await _testFixture.Connection.QueryAsync<DataExchange>(
                "[dbo].[usp_Get_DataExchangeKeyValue]",
                new { JobId = 1, Item = 2, Actor = 1 },
                commandType: CommandType.StoredProcedure)).Single();
            de.Value.Should().Be(expectedValue);
        }

        [Fact]
        public async Task TestGet()
        {
            const string key = "1_2_2";
            const string expectedValue = "Test Data";
            var loggerMock = new Mock<ILogger>();

            await _testFixture.Connection.ExecuteAsync(
                "[dbo].[usp_Set_DataExchangeJobValues]",
                new { JobId = 1, Item = 2, Actor = 2, Value = expectedValue },
                commandType: CommandType.StoredProcedure);

            var service = new SqlServerKeyValuePersistenceService(_testFixture.Config, loggerMock.Object);
            string ret = await service.GetAsync(key);

            ret.Should().Be(expectedValue);
        }

        [Fact]
        public async Task TestRemove()
        {
            const string key = "1_2_3";
            const string expectedValue = "Test Data";
            var loggerMock = new Mock<ILogger>();

            await _testFixture.Connection.ExecuteAsync(
                "[dbo].[usp_Set_DataExchangeJobValues]",
                new { JobId = 1, Item = 2, Actor = 3, Value = expectedValue },
                commandType: CommandType.StoredProcedure);

            var service = new SqlServerKeyValuePersistenceService(_testFixture.Config, loggerMock.Object);
            await service.RemoveAsync(key);

            IEnumerable<DataExchange> de = await _testFixture.Connection.QueryAsync<DataExchange>(
                "[dbo].[usp_Get_DataExchangeKeyValue]",
                new {JobId = 1, Item = 2, Actor = 1},
                commandType: CommandType.StoredProcedure);
            de.Should().BeEmpty();
        }
    }
}
