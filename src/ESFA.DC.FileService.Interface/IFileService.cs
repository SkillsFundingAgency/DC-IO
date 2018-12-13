using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ESFA.DC.FileService.Interface
{
    public interface IFileService
    {
        Task<Stream> OpenReadStreamAsync(string fileReference, string container, CancellationToken cancellationToken);

        Task<Stream> OpenWriteStreamAsync(string fileReference, string container, CancellationToken cancellationToken);

        Task<IEnumerable<string>> GetFileReferencesAsync(string container, CancellationToken cancellationToken);
    }
}
