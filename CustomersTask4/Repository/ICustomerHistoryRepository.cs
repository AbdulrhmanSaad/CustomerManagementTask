using CustomersTask4.Domain;
using CustomersTask4.DTO;

namespace CustomersTask4.Repository
{
    public interface ICustomerHistoryRepository:IGenericRepository<Customer>
    {
        Task<IEnumerable<Customer>> GetAllCustomerHistory(int customerId);
        Task<IEnumerable<AddressDto>> GetAllCustomerAddressHistory(int customerId);
    }
}
