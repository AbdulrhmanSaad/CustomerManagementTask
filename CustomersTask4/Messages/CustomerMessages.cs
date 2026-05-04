namespace CustomersTask4.Messages
{
    public class AddressMessage
    {
        public string AddressName { get; init; } = default!;
        public string AddressType { get; init; } = default!;
    }
    public abstract class BaseCustomerMessage
    {
        public string Id { get; init; } = default!;
        public string Name { get; init; } = default!;
        public string Phone { get; init; } = default!;
        public List<AddressMessage> Addresses { get; init; } = [];
    }
    public class CustomerCreatedMessage : BaseCustomerMessage
    { 
        public DateTime CreatedAt { get; init; }
        public string CreatedBy { get; init; } = default!;
    }

    public class CustomerUpdatedMessage: BaseCustomerMessage
    {
        public DateTime? ChangedAt { get; init; }
        public string? ChangedBy { get; init; }
    }

    public class CustomerDeletedMessage
    {
        public string Id { get; init; } = default!;
    }
}