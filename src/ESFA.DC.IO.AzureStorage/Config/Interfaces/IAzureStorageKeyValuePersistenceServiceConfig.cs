namespace ESFA.DC.IO.AzureStorage.Config.Interfaces
{
    public interface IAzureStorageKeyValuePersistenceServiceConfig
    {
        /// <summary>
        /// Gets the connection string used to connect to the storage account.
        /// </summary>
        string ConnectionString { get; }

        /// <summary>
        /// Gets the container name in the storage account. If empty or null this will default to persistence.
        /// </summary>
        string ContainerName { get; }
    }
}
