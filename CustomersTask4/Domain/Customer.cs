namespace CustomersTask4.Domain
{
    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; }=default!;
        public string Phone { get; set; } = default!;
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; } = default!;

        public DateTime? ChangedAt { get; set; }
        public string? ChangedBy { get; set; } = default!;
        public  List<Address> Addresses { get; set; }=new List<Address>();
        public List<CustomerHistory> History { get; set; } = new List<CustomerHistory>();
    }
}
