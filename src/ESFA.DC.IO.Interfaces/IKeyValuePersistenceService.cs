using System.Threading.Tasks;

namespace ESFA.DC.IO.Interfaces
{
    public interface IKeyValuePersistenceService
    {
        Task SaveAsync(string key, string value);

        Task<string> GetAsync(string key);

        Task RemoveAsync(string key);

        Task<bool> ContainsAsync(string key);
    }
}
