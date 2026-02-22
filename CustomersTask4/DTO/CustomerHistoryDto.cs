using CustomersTask4.Domain;

namespace CustomersTask4.DTO
{
    public class CustomerHistoryDto
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public string Action { get; set; }

        public CustomerSnapshotDto OldValues { get; set; }
        public CustomerSnapshotDto NewValues { get; set; }

        public DateTime ChangedAt { get; set; }

        public string UserEmail { get; set; }
        public string UserRole { get; set; }
    }
    public class CustomerSnapshotDto
    {
        public string Name { get; set; }
        public string Phone { get; set; }

        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }

        public List<AddressDto> Addresses { get; set; }
    }
    public class AddressDto
    {
        public string AddressType { get; set; }
        public string AddressName { get; set; }
    }

    public class CustomerHistoryResponse
    {
        public string Name { get; set; }
        public string Phone { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
    }
}
