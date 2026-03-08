using CustomersTask4.DTO;
using CustomersTask4.Abstraction;
using MediatR;
namespace CustomersTask4.CustomerHandler.Query.GetCustomerAddressesHistory
{
    public class GetCustomerAddressesHistoryQuery(string customerId) : IRequest<IEnumerable<AddressDto>>
    {
        public string CustomerId { get; } = customerId;
    }
}
