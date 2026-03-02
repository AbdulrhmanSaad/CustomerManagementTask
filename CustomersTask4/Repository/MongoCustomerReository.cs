using CustomersTask4.Data;
using CustomersTask4.Domain;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace CustomersTask4.Repository
{
    public class MongoCustomerReository : IGenericRepository<Customer>
    {
        private readonly IMongoCollection<Customer> db;
        public MongoCustomerReository(IMongoClient client,IOptions<MongoDbSetting>settings)
        {
            var database = client.GetDatabase(settings.Value.DatabaseName);
            db = database.GetCollection<Customer>("Customers");
        }
        public async Task Add(Customer entity)
        {
            await db.InsertOneAsync(entity);
        }

        public async Task Delete(Customer entity)
        {
            await db.DeleteOneAsync(c => c.Id == entity.Id);    
        }

        public  List<Customer> GetAll(params Expression<Func<Customer, object>>[] includes)
        {
            return db.Find(_ => true).ToList();
        }
       

        public async Task<Customer?> GetByIdAsync(string id, params Expression<Func<Customer, object>>[] includes)
        {
            return await db.Find(c => c.Id.Equals(id)).FirstOrDefaultAsync();
        }

        public bool PhoneExistsAsync(string phoneNumber)
        {
            var exists= db.Find(c => c.Phone == phoneNumber).FirstOrDefault();
            return exists != null;

        }

        public async Task Update(Customer entity)
        {
            await db.ReplaceOneAsync(c => c.Id == entity.Id, entity);
        }

       
    }
}
