namespace ESFA.DC.IO.SqlServer.Model
{
    public sealed class DataExchange
    {
        public long DataExchange_Id { get; set; }

        public long Job_Id { get; set; }

        public int Item { get; set; }

        public int ActorId { get; set; }

        public string Value { get; set; }
    }
}
