using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CustomersTask4.Domain
{
    public class Customer
    {
        [BsonId]
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        public string Id { get; set; }= ObjectId.GenerateNewId().ToString();
        public string Name { get; set; }=default!;
        public string Phone { get; set; } = default!;
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; } = default!;

        public DateTime? ChangedAt { get; set; } = DateTime.UtcNow;
        public string? ChangedBy { get; set; } = default!;
        public  List<Address> Addresses { get; set; }=new List<Address>();


        

    }
    
}
