using Microsoft.WindowsAzure.Storage.Table;

namespace ESFA.DC.IO.AzureTableStorage.Model
{
    public sealed class DataExchange : TableEntity
    {
        public DataExchange(string key, string value)
        {
            Key = key;
            Value = value;

            TableKey tableKey = new TableKey(key);
            PartitionKey = tableKey.JobId.ToString();
            RowKey = key;
        }

        public DataExchange()
        {
        }

        public string Key { get; set; }

        public string Value { get; set; }
    }
}
