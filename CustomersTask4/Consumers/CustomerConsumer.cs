using CustomersTask4.Data;
using CustomersTask4.Domain;
using CustomersTask4.Messages;
using MassTransit;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MapsterMapper;

namespace CustomersTask4.Consumers
{
    public class CustomerConsumer(
        IMongoClient mongoClient,
        IOptions<MongoDbSetting> mongoSettings,
        ILogger<CustomerConsumer> logger,
        IMapper mapper)
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
            var res=mapper.Map<Customer>(msg);

            await collection.InsertOneAsync(res);
            logger.LogInformation("Customer {Name} created in MongoDB", res.Name);
        }

        //DELETE
        public async Task Consume(ConsumeContext<CustomerDeletedMessage> context)
        {
            var msg = context.Message;
            var collection = GetCollection();
            if (msg.Id != null)
            {
                var result = await collection.DeleteOneAsync(c => c.Id == msg.Id);

                if (result.DeletedCount == 0)
                    logger.LogWarning("CustomerDeletedMessage — customer id={Phone} not found in MongoDB", msg.Id);
                else
                    logger.LogInformation("Customer {Phone} deleted from MongoDB", msg.Id);
            }
        }


        //UPDATE
        public async Task Consume(ConsumeContext<CustomerUpdatedMessage> context)
        {
            var msg = context.Message;
            var collection = GetCollection();

            var existing = await collection.Find(c => c.Id == msg.Id).FirstOrDefaultAsync();

            if (existing != null)
            {
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
                    logger.LogWarning("CustomerUpdatedMessage — customer {Phone} not found in MongoDB", msg.Phone);
                else
                    logger.LogInformation("Customer {Phone} updated in MongoDB", msg.Phone);
            }
        }

       
    }
}