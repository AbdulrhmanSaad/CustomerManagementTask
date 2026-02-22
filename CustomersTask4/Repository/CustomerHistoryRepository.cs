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
        public async Task<IEnumerable<Customer>> GetAllCustomerHistory(int customerId)
        {
            var res =await db.Customers.TemporalAll()
                .Where(c => c.Id == customerId).ToListAsync();
           // var res=await db.CustomerHistories.Where(c => c.CustomerId == customerId).ToListAsync();
            return res;
        }
        public async Task<IEnumerable<Address>> GetAllCustomerAddressHistory(int customerId)
        {
            var res = await db.Addresses.TemporalAll()
                .Where(c => c.CustomerId == customerId).ToListAsync();
            // var res=await db.CustomerHistories.Where(c => c.CustomerId == customerId).ToListAsync();
            return res;
        }
    }
}
