using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using Dapper;
using ESFA.DC.IO.SqlServer.Config.Interfaces;
using Moq;

namespace ESFA.DC.IO.SqlServer.Test
{
    public sealed class TestFixture : IDisposable
    {
        public ISqlServerKeyValuePersistenceServiceConfig Config { get; }

        public SqlConnection Connection { get; }

        public TestFixture()
        {
            string connectionString = ConfigurationManager.AppSettings["ConnectionStringSqlServer"];

            var mock = new Mock<ISqlServerKeyValuePersistenceServiceConfig>();
            mock.SetupGet(x => x.ConnectionString).Returns(connectionString);
            Config = mock.Object;

            Connection = new SqlConnection(connectionString);
        }

        public void Dispose()
        {
            Connection.Execute("[dbo].[usp_Remove_DataExchangeJobAllValues]",
                new { JobId = 1 },
                commandType: CommandType.StoredProcedure);
        }
    }
}
