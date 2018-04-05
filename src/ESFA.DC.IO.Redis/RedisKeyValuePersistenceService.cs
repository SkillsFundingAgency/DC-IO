using System;
using System.Net;
using System.Threading.Tasks;
using ESFA.DC.IO.Interfaces;
using ESFA.DC.IO.Redis.Config.Interfaces;
using ESFA.DC.Logging;
using ESFA.DC.Logging.Interfaces;
using StackExchange.Redis;

namespace ESFA.DC.IO.Redis
{
    public class RedisKeyValuePersistenceService : IKeyValuePersistenceService
    {
        private readonly IRedisKeyValuePersistenceServiceConfig _keyValuePersistenceServiceConfig;

        private readonly ILogger _logger;

        private ConnectionMultiplexer connection;

        public RedisKeyValuePersistenceService(IRedisKeyValuePersistenceServiceConfig keyValuePersistenceServiceConfig, ILogger logger)
        {
            _keyValuePersistenceServiceConfig = keyValuePersistenceServiceConfig;
            _logger = logger;
        }

        public async Task<string> GetAsync(string key)
        {
            RedisValue ret;
            using (new TimedLogger(_logger, "Redis Get"))
            {
                IDatabase db = await InitConnectionAsync();
                ret = await db.StringGetAsync(key);
                if (ret.IsNullOrEmpty)
                {
                    return string.Empty;
                }
            }

            return ret.ToString();
        }

        public async Task RemoveAsync(string key)
        {
            using (new TimedLogger(_logger, "Redis Remove"))
            {
                IDatabase db = await InitConnectionAsync();
                await db.KeyDeleteAsync(key);
            }
        }

        public async Task SaveAsync(string key, string value)
        {
            using (new TimedLogger(_logger, "Redis Set"))
            {
                IDatabase db = await InitConnectionAsync();
                await db.StringSetAsync(key, value);
            }
        }

        private async Task<IDatabase> InitConnectionAsync()
        {
            if (connection == null)
            {
                string[] tokens = _keyValuePersistenceServiceConfig.ConnectionString.Split(',');
                ServicePoint tableServicePoint = ServicePointManager.FindServicePoint(new Uri(tokens[0]));
                tableServicePoint.ConnectionLimit = 1000;
                connection = await ConnectionMultiplexer.ConnectAsync(_keyValuePersistenceServiceConfig.ConnectionString);
            }

            return connection.GetDatabase();
        }
    }
}
