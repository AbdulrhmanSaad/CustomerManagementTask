using CustomersTask4.DTO;
using Mediator;

namespace CustomersTask4.CustomerHandler.Query.GetCustomerAddressesHistory
{
    public class GetCustomerAddressesHistoryQuery(string customerId) : IRequest<IEnumerable<AddressDto>>
    {
        public string CustomerId { get; } = customerId;
    }
}
