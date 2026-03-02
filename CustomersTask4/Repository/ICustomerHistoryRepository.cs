using CustomersTask4.Domain;
using CustomersTask4.DTO;

namespace CustomersTask4.Repository
{
    public interface ICustomerHistoryRepository:IGenericRepository<Customer>
    {
        Task<IEnumerable<Customer>> GetAllCustomerHistory(string customerId);
        Task<IEnumerable<AddressDto>> GetAllCustomerAddressHistory(string customerId);
    }
}
