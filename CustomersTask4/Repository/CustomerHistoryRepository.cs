using CustomersTask4.Data;
using CustomersTask4.Domain;
using CustomersTask4.DTO;
using Microsoft.EntityFrameworkCore;

namespace CustomersTask4.Repository
{
    public class CustomerHistoryRepository : GenericRepository<Customer>, ICustomerHistoryRepository
    {
        private readonly ApplicationDbContext db;

        public CustomerHistoryRepository(ApplicationDbContext db):base(db)
        {
            this.db = db;
        }
        public async Task<IEnumerable<Customer>> GetAllCustomerHistory(string customerId)
        {
            var res =await db.Customers.TemporalAll()
                .Where(c => c.Id.Equals(customerId)).ToListAsync();
            
            return res;
        }
        public async Task<IEnumerable<AddressDto>> GetAllCustomerAddressHistory(string customerId)
        {
            var res = await db.Addresses.TemporalAll()
                .Where(c => c.CustomerId.Equals(customerId)).Select(e => new AddressDto{ AddressType = e.AddressType.ToString(), AddressName = e.AddressName.ToString() }).ToListAsync();
            // var res=await db.CustomerHistories.Where(c => c.CustomerId == customerId).ToListAsync();
            return res;
        }
    }
}
