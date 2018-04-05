using System;
using System.Configuration;
using ESFA.DC.IO.Redis.Config.Interfaces;
using Moq;
using StackExchange.Redis;

namespace ESFA.DC.IO.Redis.Test
{
    public sealed class TestFixture : IDisposable
    {
        public IRedisKeyValuePersistenceServiceConfig Config { get; }

        public IDatabase Database { get; }

        public TestFixture()
        {
            string connectionString = ConfigurationManager.AppSettings["ConnectionStringRedis"];

            var mock = new Mock<IRedisKeyValuePersistenceServiceConfig>();
            mock.SetupGet(x => x.ConnectionString).Returns(connectionString);
            Config = mock.Object;

            ConnectionMultiplexer connection = ConnectionMultiplexer.Connect(connectionString);
            Database = connection.GetDatabase();
        }

        public void Dispose()
        {
            string[] keys =
            {
                "1_2_3_Get",
                "1_2_3_Set",
                "1_2_3_Remove",
            };

            foreach (string key in keys)
            {
                if (Database.KeyExists(key))
                {
                    Database.KeyDelete(key);
                }
            }
        }
    }
}
