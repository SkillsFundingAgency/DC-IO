using System.Threading.Tasks;

namespace ESFA.DC.IO.Interfaces
{
    public interface IKeyValuePersistenceService
    {
        Task<bool> SaveAsync(string key, string value);

        Task<string> GetAsync(string key);

        Task<bool> RemoveAsync(string key);
    }
}
