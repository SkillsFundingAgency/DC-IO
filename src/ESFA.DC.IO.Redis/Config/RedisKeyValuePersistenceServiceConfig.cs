using System;
using ESFA.DC.IO.Redis.Config.Interfaces;

namespace ESFA.DC.IO.Redis.Config
{
    public sealed class RedisKeyValuePersistenceServiceConfig : IRedisKeyValuePersistenceServiceConfig
    {
        public string ConnectionString { get; set; }

        public TimeSpan? KeyExpiry { get; set; }
    }
}
