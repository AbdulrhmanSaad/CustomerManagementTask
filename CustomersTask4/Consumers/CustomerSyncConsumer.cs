using CustomersTask4.Data;
using CustomersTask4.Domain;
using CustomersTask4.Messages;
using MassTransit;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace CustomersTask4.Consumers
{
    public class CustomerSyncConsumer(
        IMongoClient mongoClient,
        IOptions<MongoDbSetting> mongoSettings,
        ILogger<CustomerSyncConsumer> logger)
        : IConsumer<CustomerCreatedMessage>,
          IConsumer<CustomerUpdatedMessage>,
          IConsumer<CustomerDeletedMessage>
    {
        private IMongoCollection<Customer> GetCollection()
        {
            var db = mongoClient.GetDatabase(mongoSettings.Value.DatabaseName);
            return db.GetCollection<Customer>("Customers");
        }

        //CREATE
        public async Task Consume(ConsumeContext<CustomerCreatedMessage> context)
        {
            var msg = context.Message;
            var collection = GetCollection();

            var exists = await collection.Find(c => c.Phone == msg.Phone).AnyAsync();
            if (exists)
            {
                logger.LogInformation("Customer {Phone} already in MongoDB", msg.Phone);
                return;
            }

            var customer = new Customer
            {
                Id = ObjectId.GenerateNewId().ToString(),
                Name = msg.Name,
                Phone = msg.Phone,
                CreatedAt = msg.CreatedAt,
                CreatedBy = msg.CreatedBy,
                Addresses = msg.Addresses
                    .Select(a => new Address
                    {
                        AddressName = a.AddressName,
                        AddressType = Enum.Parse<AddressType>(a.AddressType)
                    }).ToList()
            };

            await collection.InsertOneAsync(customer);
            logger.LogInformation("Customer {Name} created in MongoDB", customer.Name);
        }

        // ── UPDATE ──────────────────────────────────────────────
        public async Task Consume(ConsumeContext<CustomerUpdatedMessage> context)
        {
            var msg = context.Message;
            var collection = GetCollection();

            var update = Builders<Customer>.Update
                .Set(c => c.Name, msg.Name)
                .Set(c => c.Phone, msg.Phone)
                .Set(c => c.ChangedAt, msg.ChangedAt)
                .Set(c => c.ChangedBy, msg.ChangedBy)
                .Set(c => c.Addresses, msg.Addresses
                    .Select(a => new Address
                    {
                        AddressName = a.AddressName,
                        AddressType = Enum.Parse<AddressType>(a.AddressType)
                    }).ToList());

            var result = await collection.UpdateOneAsync(
                c => c.Phone == msg.Phone,
                update);

            if (result.MatchedCount == 0)
                logger.LogWarning("CustomerUpdatedMessage — customer {Phone} not found in MongoDB", msg.Phone);
            else
                logger.LogInformation("Customer {Phone} updated in MongoDB", msg.Phone);
        }

        // ── DELETE ──────────────────────────────────────────────
        public async Task Consume(ConsumeContext<CustomerDeletedMessage> context)
        {
            var msg = context.Message;
            var collection = GetCollection();

            var result = await collection.DeleteOneAsync(c => c.Phone == msg.Phone);

            if (result.DeletedCount == 0)
                logger.LogWarning("CustomerDeletedMessage — customer {Phone} not found in MongoDB", msg.Phone);
            else
                logger.LogInformation("Customer {Phone} deleted from MongoDB", msg.Phone);
        }
    }
}