using Newtonsoft.Json;

namespace ESFA.DC.IO.AzureCosmos.Model
{
    public sealed class DataExchange
    {
        public DataExchange()
        {
        }

        public DataExchange(string key, string value)
        {
            Id = key;
            Value = value;
        }

        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        public string Value { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
