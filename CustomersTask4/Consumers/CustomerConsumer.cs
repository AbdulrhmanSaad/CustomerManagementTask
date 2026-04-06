using CustomersTask4.Data;
using CustomersTask4.Domain;
using CustomersTask4.Messages;
using MapsterMapper;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace CustomersTask4.Consumers
{
    public class CustomerConsumer(
        IMongoClient mongoClient,
        IOptions<MongoDbSetting> mongoSettings,
        ILogger<CustomerConsumer> logger,
        IMapper mapper)
    {
        private IMongoCollection<Customer> GetCollection()
        {
            var db = mongoClient.GetDatabase(mongoSettings.Value.DatabaseName);
            return db.GetCollection<Customer>("Customers");
        }

        // CREATE
        public async Task Handle(CustomerCreatedMessage msg)
        {
            var collection = GetCollection();
            var customer = mapper.Map<Customer>(msg);
            await collection.InsertOneAsync(customer);
            logger.LogInformation("Customer {Name} created in MongoDB", customer.Name);
        }

        // DELETE
        public async Task Handle(CustomerDeletedMessage msg)
        {
            if (msg.Id is null) return;

            var collection = GetCollection();
            Customer? result =(Customer?)collection.Find(c => c.Id == msg.Id);
            if (result is null)
            {
                logger.LogWarning("CustomerDeletedMessage — customer id={Id} not found in MongoDB", msg.Id);
            }
            else
            {
                result.IsDeleted = true;
                logger.LogInformation("Customer {Id} deleted from MongoDB", msg.Id);
            }
        }

        // UPDATE
        public async Task Handle(CustomerUpdatedMessage msg)
        {
            var collection = GetCollection();
            var existing = await collection.Find(c => c.Id == msg.Id).FirstOrDefaultAsync();

            if (existing is null) return;

            existing.Name = msg.Name;
            existing.Phone = msg.Phone;
            existing.ChangedAt = msg.ChangedAt;
            existing.ChangedBy = msg.ChangedBy;
            existing.Addresses = msg.Addresses
                .Select(a => new Address
                {
                    AddressName = a.AddressName,
                    AddressType = Enum.Parse<AddressType>(a.AddressType)
                }).ToList();

            var result = await collection.ReplaceOneAsync(c => c.Id == msg.Id, existing);

            if (result.MatchedCount == 0)
                logger.LogWarning("CustomerUpdatedMessage — customer {Id} not found in MongoDB", msg.Id);
            else
                logger.LogInformation("Customer {Id} updated in MongoDB", msg.Id);
        }
    }
}