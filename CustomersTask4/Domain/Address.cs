namespace CustomersTask4.Domain
{
    public class Address
    {
        public int Id { get; set; }
        public AddressType AddressType { get; set; } = AddressType.Home;
        public string AddressName { get; set; } = default!;
        public int CustomerId { get; set; }
    }
    public enum AddressType
    {
        Home,
        Work,
    }
}
