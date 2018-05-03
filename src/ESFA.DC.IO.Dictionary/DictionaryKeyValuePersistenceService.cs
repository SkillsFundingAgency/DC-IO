using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using ESFA.DC.IO.Interfaces;

namespace ESFA.DC.IO.Dictionary
{
    public sealed class DictionaryKeyValuePersistenceService : IKeyValuePersistenceService
    {
#pragma warning disable SA1401 // Fields must be private (internal to enable testings)
        // ReSharper disable once MemberCanBePrivate.Global
        internal readonly ConcurrentDictionary<string, string> _dictionary;
#pragma warning restore SA1401 // Fields must be private

        public DictionaryKeyValuePersistenceService()
        {
            _dictionary = new ConcurrentDictionary<string, string>();
        }

        public async Task SaveAsync(string key, string value)
        {
            _dictionary[key] = value;
        }

        public async Task<string> GetAsync(string key)
        {
            if (_dictionary.TryGetValue(key, out string value))
            {
                return value;
            }

            throw new KeyNotFoundException($"Key '{key}' was not found in the store");
        }

        public async Task RemoveAsync(string key)
        {
            if (!_dictionary.TryRemove(key, out string _))
            {
                throw new KeyNotFoundException($"Key '{key}' was not found in the store");
            }
        }

        public async Task<bool> ContainsAsync(string key)
        {
            return _dictionary.ContainsKey(key);
        }
    }
}
