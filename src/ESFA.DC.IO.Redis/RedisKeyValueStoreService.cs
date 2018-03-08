using System.Threading.Tasks;
using ESFA.DC.IO.Interfaces;
using ESFA.DC.IO.Redis.Config.Interfaces;

namespace ESFA.DC.IO.Redis
{
    public class RedisKeyValueStoreService : IKeyValueStoreService
    {
        private IRedisKeyValueStoreServiceConfig _keyValueStoreServiceConfig;

        public RedisKeyValueStoreService(IRedisKeyValueStoreServiceConfig keyValueStoreServiceConfig)
        {
            _keyValueStoreServiceConfig = keyValueStoreServiceConfig;
        }

        public Task<string> GetAsync(string key)
        {
            throw new System.NotImplementedException();
        }

        public Task<bool> RemoveAsync(string key)
        {
            throw new System.NotImplementedException();
        }

        public Task<bool> SaveAsync(string key, string value)
        {
            throw new System.NotImplementedException();
        }
    }
}
