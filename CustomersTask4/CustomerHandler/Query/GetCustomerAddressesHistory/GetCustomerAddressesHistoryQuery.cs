using CustomersTask4.DTO;
using MediatR;

namespace CustomersTask4.CustomerHandler.Query.GetCustomerAddressesHistory
{
    public class GetCustomerAddressesHistoryQuery(int customerId) : IRequest<IEnumerable<AddressDto>>
    {
        public int CustomerId { get; } = customerId;
    }
}
