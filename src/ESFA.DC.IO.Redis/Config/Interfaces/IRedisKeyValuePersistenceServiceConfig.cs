using System;

namespace ESFA.DC.IO.Redis.Config.Interfaces
{
   public interface IRedisKeyValuePersistenceServiceConfig
    {
        string ConnectionString { get; }

        TimeSpan? KeyExpiry { get; }
    }
}
