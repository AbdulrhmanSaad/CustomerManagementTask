using CustomersTask4.Domain;
using CustomersTask4.DTO;
using MediatR;

namespace CustomersTask4.CustomerHandler.Query
{
    public class GetAllCustomerQuery:IRequest<IEnumerable<CustomerDto>>
    {

    }
}
