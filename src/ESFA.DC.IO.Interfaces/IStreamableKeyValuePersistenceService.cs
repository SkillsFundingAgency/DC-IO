using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ESFA.DC.IO.Interfaces
{
    public interface IStreamableKeyValuePersistenceService : IKeyValuePersistenceService
    {
        Task SaveAsync(string key, Stream value, CancellationToken cancellationToken = default(CancellationToken));

        Task GetAsync(string key, Stream value, CancellationToken cancellationToken = default(CancellationToken));
    }
}
