using CustomersTask4.Domain;

namespace CustomersTask4.DTO
{
    public class CustomerDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public DateTime ChangedAt { get; set; }
        public string ChangedBy { get; set; }
        public string AddressType { get; set; }
        public string HomeAddressLocation { get; set; }
        public string AddressType2 { get; set; }
        public string WorkAddressLocation { get; set; }
        
    }
}
