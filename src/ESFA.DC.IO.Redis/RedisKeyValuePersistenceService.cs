using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.IO.Interfaces;
using ESFA.DC.IO.Redis.Config.Interfaces;
using StackExchange.Redis;

namespace ESFA.DC.IO.Redis
{
    public class RedisKeyValuePersistenceService : IKeyValuePersistenceService
    {
        private readonly SemaphoreSlim _initLock = new SemaphoreSlim(1, 1);

        private readonly IRedisKeyValuePersistenceServiceConfig _keyValuePersistenceServiceConfig;

        private ConnectionMultiplexer connection;

        public RedisKeyValuePersistenceService(IRedisKeyValuePersistenceServiceConfig keyValuePersistenceServiceConfig)
        {
            _keyValuePersistenceServiceConfig = keyValuePersistenceServiceConfig;
        }

        public async Task<string> GetAsync(string key, CancellationToken cancellationToken = default(CancellationToken))
        {
            IDatabase db = await InitConnectionAsync(cancellationToken);

            if (cancellationToken.IsCancellationRequested)
            {
                return null;
            }

            var ret = await db.StringGetAsync(key);
            return ret.IsNullOrEmpty ? string.Empty : ret.ToString();
        }

        public async Task RemoveAsync(string key, CancellationToken cancellationToken = default(CancellationToken))
        {
            IDatabase db = await InitConnectionAsync(cancellationToken);

            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            await db.KeyDeleteAsync(key);
        }

        public async Task SaveAsync(string key, string value, CancellationToken cancellationToken = default(CancellationToken))
        {
            IDatabase db = await InitConnectionAsync(cancellationToken);

            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            await db.StringSetAsync(key, value);
            await db.KeyExpireAsync(key, _keyValuePersistenceServiceConfig.KeyExpiry ?? TimeSpan.FromDays(7));
        }

        public async Task<bool> ContainsAsync(string key, CancellationToken cancellationToken = default(CancellationToken))
        {
            IDatabase db = await InitConnectionAsync(cancellationToken);

            if (cancellationToken.IsCancellationRequested)
            {
                return false;
            }

            return db.KeyExists(key);
        }

        private async Task<IDatabase> InitConnectionAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            await _initLock.WaitAsync(cancellationToken);

            try
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return null;
                }

                if (connection == null)
                {
                    string[] tokens = _keyValuePersistenceServiceConfig.ConnectionString.Split(',');
                    ServicePoint tableServicePoint = ServicePointManager.FindServicePoint(new Uri(tokens[0]));
                    tableServicePoint.ConnectionLimit = 1000;
                    connection = await ConnectionMultiplexer.ConnectAsync(_keyValuePersistenceServiceConfig.ConnectionString);
                }

                return connection.GetDatabase();
            }
            finally
            {
                _initLock.Release();
            }
        }
    }
}
