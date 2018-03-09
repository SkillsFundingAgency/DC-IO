using System.IO;
using System.Threading.Tasks;
using ESFA.DC.IO.Interfaces;

namespace ESFA.DC.IO.FileSystem
{
    public class FileSystemKeyValuePersistenceService : IKeyValuePersistenceService
    {
        public Task SaveAsync(string key, string value)
        {
            return Task.Run(() => File.WriteAllText(key, value));
        }

        public Task<string> GetAsync(string key)
        {
            return Task.Run(() => File.ReadAllText(key));
        }

        public Task RemoveAsync(string key)
        {
            return Task.Run(() => File.Delete(key));
        }
    }
}
