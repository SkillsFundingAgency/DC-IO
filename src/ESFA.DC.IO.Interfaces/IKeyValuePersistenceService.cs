using System.Threading;
using System.Threading.Tasks;

namespace ESFA.DC.IO.Interfaces
{
    public interface IKeyValuePersistenceService
    {
        Task SaveAsync(string key, string value, CancellationToken cancellationToken = default(CancellationToken));

        Task<string> GetAsync(string key, CancellationToken cancellationToken = default(CancellationToken));

        Task RemoveAsync(string key, CancellationToken cancellationToken = default(CancellationToken));

        Task<bool> ContainsAsync(string key, CancellationToken cancellationToken = default(CancellationToken));
    }
}
