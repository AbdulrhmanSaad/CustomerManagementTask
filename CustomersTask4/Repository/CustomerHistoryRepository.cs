using CustomersTask4.Data;
using CustomersTask4.Domain;
using CustomersTask4.DTO;
using Microsoft.EntityFrameworkCore;

namespace CustomersTask4.Repository
{
    public class CustomerHistoryRepository : ICustomerHistoryRepository
    {
        private readonly ApplicationDbContext db;

        public CustomerHistoryRepository(ApplicationDbContext db)
        {
            this.db = db;
        }
        public async Task<IEnumerable<CustomerHistory>> GetAllCustomerHistory(int customerId)
        {
            var res=await db.CustomerHistories.Where(c => c.CustomerId == customerId).ToListAsync();
            return res;
        }
    }
}
