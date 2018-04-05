namespace ESFA.DC.IO.AzureCosmos.Config.Interfaces
{
    public interface IAzureCosmosKeyValuePersistenceServiceConfig
    {
        string EndpointUrl { get; }

        string AuthKeyOrResourceToken { get; }
    }
}
