using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CustomersTask4.Domain
{
    public class CustomerHistoryDocument
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string CustomerId { get; set; } = default!;
        public Customer Snapshot { get; set; } = default!;

        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
    }
}