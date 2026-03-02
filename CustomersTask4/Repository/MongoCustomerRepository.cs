using CustomersTask4.Data;
using CustomersTask4.Domain;
using CustomersTask4.DTO;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Linq.Expressions;

namespace CustomersTask4.Repository
{
    public class MongoCustomerRepository : ICustomerHistoryRepository
    {
        private readonly IMongoCollection<Customer> _customers;
        private readonly IMongoCollection<CustomerHistoryDocument> _history;

        public MongoCustomerRepository(IMongoClient client, IOptions<MongoDbSetting> settings)
        {
            var database = client.GetDatabase(settings.Value.DatabaseName);
            _customers = database.GetCollection<Customer>("Customers");
            _history = database.GetCollection<CustomerHistoryDocument>("CustomersHistory");
        }


        public async Task<IEnumerable<Customer>> GetAllCustomerHistory(string customerId)
        {
            var historyDocs = await _history
                .Find(h => h.CustomerId == customerId)
                .SortByDescending(h => h.PeriodEnd)
                .ToListAsync();

            return historyDocs.Select(h => h.Snapshot);
        }

        public async Task<IEnumerable<AddressDto>> GetAllCustomerAddressHistory(string customerId)
        {
            var historyDocs = await _history
                .Find(h => h.CustomerId == customerId)
                .SortByDescending(h => h.PeriodEnd)
                .ToListAsync();

            return historyDocs
                .SelectMany(h => h.Snapshot.Addresses)
                .Select(a => new AddressDto
                {
                    AddressType = a.AddressType.ToString(),
                    AddressName = a.AddressName
                });
        }

        public async Task Add(Customer entity)
        {
            await _customers.InsertOneAsync(entity);
        }

        public async Task Update(Customer entity)
        {
            var existing = await _customers.Find(c => c.Id == entity.Id).FirstOrDefaultAsync();
            if (existing != null)
            {
                await _history.InsertOneAsync(new CustomerHistoryDocument
                {
                    CustomerId = existing.Id,
                    Snapshot = existing,
                    PeriodStart = existing.ChangedAt ?? existing.CreatedAt,
                    PeriodEnd = DateTime.UtcNow
                });
            }

            await _customers.ReplaceOneAsync(c => c.Id == entity.Id, entity);
        }

        public async Task Delete(Customer entity)
        {
            var existing = await _customers.Find(c => c.Id == entity.Id).FirstOrDefaultAsync();
            if (existing != null)
            {
                await _history.InsertOneAsync(new CustomerHistoryDocument
                {
                    CustomerId = existing.Id,
                    Snapshot = existing,
                    PeriodStart = existing.ChangedAt ?? existing.CreatedAt,
                    PeriodEnd = DateTime.UtcNow
                });
            }

            await _customers.DeleteOneAsync(c => c.Id == entity.Id);
        }

        public List<Customer> GetAll(params Expression<Func<Customer, object>>[] includes)
        {
            return _customers.Find(_ => true).ToList();
        }

        public async Task<Customer?> GetByIdAsync(string id, params Expression<Func<Customer, object>>[] includes)
        {
            return await _customers.Find(c => c.Id == id).FirstOrDefaultAsync();
        }

        public bool PhoneExistsAsync(string phoneNumber)
        {
            var exists = _customers.Find(c => c.Phone == phoneNumber).FirstOrDefault();
            return exists != null;
        }
    }
}