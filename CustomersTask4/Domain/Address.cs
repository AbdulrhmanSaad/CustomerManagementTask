using MongoDB.Bson.Serialization.Attributes;

namespace CustomersTask4.Domain
{
    public class Address
    {
        public int Id { get; set; }
        public AddressType AddressType { get; set; } = AddressType.Home;
        public string AddressName { get; set; } = default!;
        [BsonIgnore]
        public string CustomerId { get; set; }
        
        [BsonIgnore]
        public Customer Customer { get; set; } = default!;

    }
    public enum AddressType
    {
        Home,
        Work,
    }
}
