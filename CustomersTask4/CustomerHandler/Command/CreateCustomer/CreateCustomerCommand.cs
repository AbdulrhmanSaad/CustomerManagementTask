using CustomersTask4.Domain;
using CustomersTask4.DTO;
using Mediator;
using System.Windows.Input;


namespace CustomersTask4.CustomerHandler.Command.CreateCustomer
{
    public class CreateCustomerCommand: IRequest
    {
        public string Name { get; set; }
        public string Phone { get; set; }

        public List<AddressDtoEnum> Addresses { get; set; }= new List<AddressDtoEnum>();
       
    }


}
