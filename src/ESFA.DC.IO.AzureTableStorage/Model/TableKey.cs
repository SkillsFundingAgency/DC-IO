using System;

namespace ESFA.DC.IO.AzureTableStorage.Model
{
    public sealed class TableKey
    {
        public TableKey(string key)
        {
            string[] tokens = key.Split(new[] { '_' }, StringSplitOptions.RemoveEmptyEntries);
            JobId = Convert.ToInt32(tokens[0]);
            Item = Convert.ToInt32(tokens[1]);
            Actor = Convert.ToInt32(tokens[2]);
        }

        public TableKey(int jobId, int item, int actor)
        {
            JobId = jobId;
            Item = item;
            Actor = actor;
        }

        public int JobId { get; }

        public int Item { get; }

        public int Actor { get; }

        public override string ToString()
        {
            return $"{JobId}_{Item}_{Actor}";
        }
    }
}
