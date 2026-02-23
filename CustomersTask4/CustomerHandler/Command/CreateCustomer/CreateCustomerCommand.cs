using CustomersTask4.Domain;
using CustomersTask4.DTO;
using MediatR;

namespace CustomersTask4.CustomerHandler.Command.CreateCustomer
{
    public class CreateCustomerCommand: IRequest
    {
        public string Name { get; set; }
        public string Phone { get; set; }

        public List<AddressDtoEnum> Addresses { get; set; }= new List<AddressDtoEnum>();
        //public AddressType AddressType { get; set; }
        //public string HomeAddressLocation { get; set; }
        //public AddressType AddressType2 { get; set; }
        //public string WorkAddressLocation { get; set; }

    }
}
