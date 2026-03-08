using CustomersTask4.Domain;

namespace CustomersTask4.Messages
{
    public class AddressMessage
    {
        public string AddressName { get; init; } = default!;
        public string AddressType { get; init; } = default!;
    }

    public class CustomerCreatedMessage
    {
        public string Id { get; init; } = default!;
        public string Name { get; init; } = default!;
        public string Phone { get; init; } = default!;
        public DateTime CreatedAt { get; init; }
        public string CreatedBy { get; init; } = default!;
        public List<AddressMessage> Addresses { get; init; } = [];
    }

    public class CustomerUpdatedMessage
    {
        public string Id { get; init; } = default!;
        public string Name { get; init; } = default!;
        public string Phone { get; init; } = default!;
        public DateTime? ChangedAt { get; init; }
        public string? ChangedBy { get; init; }
        public List<AddressMessage> Addresses { get; init; } = [];
    }

    public class CustomerDeletedMessage
    {
        public string Id { get; init; } = default!;
    }
}