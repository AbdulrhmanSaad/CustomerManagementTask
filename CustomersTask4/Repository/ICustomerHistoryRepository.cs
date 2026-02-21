using CustomersTask4.Domain;
using CustomersTask4.DTO;

namespace CustomersTask4.Repository
{
    public interface ICustomerHistoryRepository
    {
        Task<IEnumerable<CustomerHistory>> GetAllCustomerHistory(int customerId);
    }
}
