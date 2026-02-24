using CustomersTask4.Domain;
using CustomersTask4.DTO;
using Mediator;

namespace CustomersTask4.CustomerHandler.Query.GetCustomerHistory
{
    public class GetCustomerHistoryQuery(int customerId) : IRequest<IEnumerable<CustomerHistoryResponse>>
    {
        public int CustomerId { get; } = customerId;
    }
}

