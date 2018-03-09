using System.Threading.Tasks;
using ESFA.DC.IO.Interfaces;
using ESFA.DC.IO.Redis.Config.Interfaces;

namespace ESFA.DC.IO.Redis
{
    public class RedisKeyValuePersistenceService : IKeyValuePersistenceService
    {
        private readonly IRedisKeyValuePersistenceServiceConfig _keyValuePersistenceServiceConfig;

        public RedisKeyValuePersistenceService(IRedisKeyValuePersistenceServiceConfig keyValuePersistenceServiceConfig)
        {
            _keyValuePersistenceServiceConfig = keyValuePersistenceServiceConfig;
        }

        public Task<string> GetAsync(string key)
        {
            throw new System.NotImplementedException();
        }

        public Task RemoveAsync(string key)
        {
            throw new System.NotImplementedException();
        }

        public Task SaveAsync(string key, string value)
        {
            throw new System.NotImplementedException();
        }
    }
}
