using CustomersTask4.Domain;
using MediatR;

namespace CustomersTask4.CustomerHandler.Command.UpdateCustomer
{
    public class UpdateCustomerCommand:IRequest
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public AddressType AddressType { get; set; }
        public string HomeAddressLocation { get; set; }
        public AddressType AddressType2 { get; set; }
        public string WorkAddressLocation { get; set; }

    }
}
