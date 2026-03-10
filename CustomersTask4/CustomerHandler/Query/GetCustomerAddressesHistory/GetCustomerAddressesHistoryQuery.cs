using CustomersTask4.DTO;
using CustomersTask4.Abstraction;
namespace CustomersTask4.CustomerHandler.Query.GetCustomerAddressesHistory
{
    public class GetCustomerAddressesHistoryQuery(string customerId) 
    {
        public string CustomerId { get; } = customerId;
    }
}
