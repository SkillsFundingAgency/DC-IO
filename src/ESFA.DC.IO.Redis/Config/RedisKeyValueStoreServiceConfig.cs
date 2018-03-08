using ESFA.DC.IO.Redis.Config.Interfaces;

namespace ESFA.DC.IO.Redis.Config
{
    public class RedisKeyValueStoreServiceConfig : IRedisKeyValueStoreServiceConfig
    {
        public string ConnectionString { get; set; }
    }
}
