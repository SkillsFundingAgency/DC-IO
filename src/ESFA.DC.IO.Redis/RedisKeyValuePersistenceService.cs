using System.Diagnostics;
using System.Threading.Tasks;
using ESFA.DC.IO.Interfaces;
using ESFA.DC.IO.Redis.Config.Interfaces;
using ESFA.DC.Logging.Interfaces;
using StackExchange.Redis;

namespace ESFA.DC.IO.Redis
{
    public class RedisKeyValuePersistenceService : IKeyValuePersistenceService
    {
        private readonly ILogger _logger;
        private readonly IRedisKeyValuePersistenceServiceConfig _keyValuePersistenceServiceConfig;

        private ConnectionMultiplexer connection = null;

        public RedisKeyValuePersistenceService(ILogger logger, IRedisKeyValuePersistenceServiceConfig keyValuePersistenceServiceConfig)
        {
            _logger = logger;
            _keyValuePersistenceServiceConfig = keyValuePersistenceServiceConfig;
        }

        public async Task<string> GetAsync(string key)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            var db = await InitConnectionAsync();
            var ret = await db.StringGetAsync(key);
            if (ret.IsNullOrEmpty)
            {
                return string.Empty;
            }

            stopwatch.Stop();
            _logger.LogDebug($"Redis Get: {stopwatch.ElapsedMilliseconds}");
            return ret.ToString();
        }

        public async Task RemoveAsync(string key)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            var db = await InitConnectionAsync();
            await db.KeyDeleteAsync(key);
            stopwatch.Stop();
            _logger.LogDebug($"Redis Delete: {stopwatch.ElapsedMilliseconds}");
        }

        public async Task SaveAsync(string key, string value)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            var db = await InitConnectionAsync();
            await db.StringSetAsync(key, value);
            stopwatch.Stop();
            _logger.LogDebug($"Redis Delete: {stopwatch.ElapsedMilliseconds}");
        }

        private async Task<IDatabase> InitConnectionAsync()
        {
            if (connection == null)
            {
                connection = await ConnectionMultiplexer.ConnectAsync(_keyValuePersistenceServiceConfig.ConnectionString);
            }

            return connection.GetDatabase();
        }
    }
}
