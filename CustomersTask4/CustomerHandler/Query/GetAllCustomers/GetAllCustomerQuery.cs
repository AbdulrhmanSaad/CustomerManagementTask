using CustomersTask4.Domain;
using CustomersTask4.DTO;
using Mediator;

namespace CustomersTask4.CustomerHandler.Query.GetAllCustomers
{
    public class GetAllCustomerQuery:IRequest<IEnumerable<CustomerDto>>
    {

    }
}
