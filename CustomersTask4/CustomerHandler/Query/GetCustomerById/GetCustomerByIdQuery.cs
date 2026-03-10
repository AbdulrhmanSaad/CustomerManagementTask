using CustomersTask4.Domain;
using CustomersTask4.DTO;
using MediatR;

namespace CustomersTask4.CustomerHandler.Query.GetCustomerById
{
    public class GetCustomerByIdQuery(string id)
    {
        public string id=id;
    }
}
