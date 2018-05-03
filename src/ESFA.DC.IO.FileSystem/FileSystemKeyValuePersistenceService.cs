using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ESFA.DC.IO.FileSystem.Config.Interfaces;
using ESFA.DC.IO.Interfaces;

namespace ESFA.DC.IO.FileSystem
{
    public class FileSystemKeyValuePersistenceService : IKeyValuePersistenceService
    {
        private readonly IFileSystemKeyValuePersistenceServiceConfig _keyValuePersistenceServiceConfig;

        public FileSystemKeyValuePersistenceService(IFileSystemKeyValuePersistenceServiceConfig keyValuePersistenceServiceConfig)
        {
            _keyValuePersistenceServiceConfig = keyValuePersistenceServiceConfig;
        }

        public async Task SaveAsync(string key, string value)
        {
            await Task.Run(() => File.WriteAllText(GetFilename(key), value));
        }

        public async Task<string> GetAsync(string key)
        {
            string filename = GetFilename(key);
            if (!File.Exists(filename))
            {
                throw new KeyNotFoundException($"Key '{key}' was not found in the store");
            }

            return File.ReadAllText(GetFilename(key));
        }

        public async Task RemoveAsync(string key)
        {
            File.Delete(GetFilename(key));
        }

        public async Task<bool> ContainsAsync(string key)
        {
            return File.Exists(GetFilename(key));
        }

        public string GetFilename(string key)
        {
            return Path.Combine(_keyValuePersistenceServiceConfig.Directory, $"{key}.dat");
        }
    }
}
