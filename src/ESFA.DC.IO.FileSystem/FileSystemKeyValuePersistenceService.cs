using System.IO;
using System.Threading.Tasks;
using ESFA.DC.IO.FileSystem.Config.Interfaces;
using ESFA.DC.IO.Interfaces;
using ESFA.DC.Logging;
using ESFA.DC.Logging.Interfaces;

namespace ESFA.DC.IO.FileSystem
{
    public class FileSystemKeyValuePersistenceService : IKeyValuePersistenceService
    {
        private readonly IFileSystemKeyValuePersistenceServiceConfig _keyValuePersistenceServiceConfig;
        private readonly ILogger _logger;

        public FileSystemKeyValuePersistenceService(IFileSystemKeyValuePersistenceServiceConfig keyValuePersistenceServiceConfig, ILogger logger)
        {
            _keyValuePersistenceServiceConfig = keyValuePersistenceServiceConfig;
            _logger = logger;
        }

        public async Task SaveAsync(string key, string value)
        {
            using (new TimedLogger(_logger, "Filesystem Set"))
            {
                await Task.Run(() => File.WriteAllText(GetFilename(key), value));
            }
        }

        public async Task<string> GetAsync(string key)
        {
            string ret;
            using (new TimedLogger(_logger, "Filesystem Set"))
            {
                ret = await Task.Run(() => File.ReadAllText(GetFilename(key)));
            }

            return ret;
        }

        public async Task RemoveAsync(string key)
        {
            using (new TimedLogger(_logger, "Filesystem Remove"))
            {
                await Task.Run(() => File.Delete(GetFilename(key)));
            }
        }

        public string GetFilename(string key)
        {
            return Path.Combine(_keyValuePersistenceServiceConfig.Directory, $"{key}.dat");
        }
    }
}
