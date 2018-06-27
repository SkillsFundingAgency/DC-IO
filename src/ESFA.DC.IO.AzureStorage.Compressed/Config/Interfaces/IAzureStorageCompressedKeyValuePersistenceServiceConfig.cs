using System.IO.Compression;
using System.Text;

namespace ESFA.DC.IO.AzureStorage.Compressed.Config.Interfaces
{
    public interface IAzureStorageCompressedKeyValuePersistenceServiceConfig
    {
        /// <summary>
        /// Gets the connection string used to connect to the storage account.
        /// </summary>
        string ConnectionString { get; }

        /// <summary>
        /// Gets the container name in the storage account. If empty or null this will default to persistence.
        /// </summary>
        string ContainerName { get; }

        /// <summary>
        ///  Gets the encoding to use for the compression/decompression
        /// </summary>
        Encoding ValueEncoding { get; }

        /// <summary>
        /// Gets the compression level to use
        /// </summary>
        CompressionLevel CompressionLevel { get; }
    }
}
