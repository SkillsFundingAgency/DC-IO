using ESFA.DC.IO.Redis.Config.Interfaces;

namespace ESFA.DC.IO.Redis.Config
{
    public class RedisKeyValuePersistenceServiceConfig : IRedisKeyValuePersistenceServiceConfig
    {
        public string ConnectionString { get; set; }
    }
}
