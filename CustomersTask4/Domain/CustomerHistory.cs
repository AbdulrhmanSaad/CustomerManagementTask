namespace CustomersTask4.Domain
{
    public class CustomerHistory
    {
        public int Id { get; set; }
        public int? CustomerId { get; set; }
        public string Action { get; set; } = default!;
        public string OldValues { get; set; } = default!;
        public string NewValues { get; set; } = default!;
        public DateTime ChangedAt { get; set; }
        public string UserId { get; set; } = default!;
        public string UserEmail { get; set; } = default!;
        public string UserRole { get; set; } = default!;
        public Customer Customer { get; set; }
    }
}
