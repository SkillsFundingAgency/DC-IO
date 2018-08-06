using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.IO.FileSystem.Config.Interfaces;
using ESFA.DC.IO.Interfaces;

namespace ESFA.DC.IO.FileSystem
{
    public class FileSystemKeyValuePersistenceService : IStreamableKeyValuePersistenceService
    {
        private readonly IFileSystemKeyValuePersistenceServiceConfig _keyValuePersistenceServiceConfig;

        public FileSystemKeyValuePersistenceService(IFileSystemKeyValuePersistenceServiceConfig keyValuePersistenceServiceConfig)
        {
            _keyValuePersistenceServiceConfig = keyValuePersistenceServiceConfig;
        }

        public async Task SaveAsync(string key, string value, CancellationToken cancellationToken = default(CancellationToken))
        {
            await Task.Run(() => File.WriteAllText(GetFilename(key), value), cancellationToken);
        }

        public async Task SaveAsync(string key, Stream value, CancellationToken cancellationToken = new CancellationToken())
        {
            using (FileStream fileWriter = new FileStream(GetFilename(key), FileMode.Create))
            {
                value.Seek(0, SeekOrigin.Begin);
                await value.CopyToAsync(fileWriter, 81920, cancellationToken);
            }
        }

        public async Task<string> GetAsync(string key, CancellationToken cancellationToken = default(CancellationToken))
        {
            string filename = GetFilename(key);
            if (!File.Exists(filename))
            {
                throw new KeyNotFoundException($"Key '{key}' was not found in the store");
            }

            return File.ReadAllText(GetFilename(key));
        }

        public async Task GetAsync(string key, Stream value, CancellationToken cancellationToken = new CancellationToken())
        {
            string filename = GetFilename(key);
            if (!File.Exists(filename))
            {
                throw new KeyNotFoundException($"Key '{key}' was not found in the store");
            }

            using (FileStream fileReader = new FileStream(GetFilename(key), FileMode.Open))
            {
                value.Seek(0, SeekOrigin.Begin);
                await fileReader.CopyToAsync(value, 81920, cancellationToken);
            }
        }

        public async Task RemoveAsync(string key, CancellationToken cancellationToken = default(CancellationToken))
        {
            File.Delete(GetFilename(key));
        }

        public async Task<bool> ContainsAsync(string key, CancellationToken cancellationToken = default(CancellationToken))
        {
            return File.Exists(GetFilename(key));
        }

        public string GetFilename(string key, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Path.Combine(_keyValuePersistenceServiceConfig.Directory, $"{key}.dat");
        }
    }
}
