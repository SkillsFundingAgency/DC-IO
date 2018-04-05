using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using ESFA.DC.IO.Interfaces;
using ESFA.DC.Logging;
using ESFA.DC.Logging.Interfaces;

namespace ESFA.DC.IO.Dictionary
{
    public sealed class DictionaryKeyValuePersistenceService : IKeyValuePersistenceService
    {
#pragma warning disable SA1401 // Fields must be private (internal to enable testings)
        // ReSharper disable once MemberCanBePrivate.Global
        internal readonly ConcurrentDictionary<string, string> _dictionary;
#pragma warning restore SA1401 // Fields must be private

        private readonly ILogger _logger;

        public DictionaryKeyValuePersistenceService(ILogger logger)
        {
            _logger = logger;
            _dictionary = new ConcurrentDictionary<string, string>();
        }

        public async Task SaveAsync(string key, string value)
        {
            using (new TimedLogger(_logger, "Dictionary Set"))
            {
                _dictionary[key] = value;
            }
        }

        public async Task<string> GetAsync(string key)
        {
            using (new TimedLogger(_logger, "Dictionary Get"))
            {
                if (_dictionary.TryGetValue(key, out string value))
                {
                    return value;
                }

                throw new KeyNotFoundException($"Key '{key}' was not found in the dictionary");
            }
        }

        public async Task RemoveAsync(string key)
        {
            using (new TimedLogger(_logger, "Dictionary Remove"))
            {
                if (!_dictionary.TryRemove(key, out string _))
                {
                    throw new KeyNotFoundException($"Key '{key}' was not found in the dictionary");
                }
            }
        }
    }
}
