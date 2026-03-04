using CustomersTask4.CustomerHandler.Command.MigrateToMongo;
using CustomersTask4.Data;
using CustomersTask4.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
namespace CustomersTask4.Services
{
    public interface IMigratetoMongo
    {
        Task<MigrateToMongoResult> MigrateFromSqlToMongo();
        Task<MigrateToMongoResult> MigrateFromMongoToSql();
    }

    public class MigrateToMongo : IMigratetoMongo
    {
        private readonly ApplicationDbContext sqlDb;
        private readonly IMongoClient mongoClient;
        private readonly IOptions<MongoDbSetting> mongoSettings;
        private readonly ILogger<MigrateToMongoCommandHandler> logger;

        public MigrateToMongo(ApplicationDbContext sqlDb,
        IMongoClient MongoClient,
        IOptions<MongoDbSetting> mongoSettings,
        ILogger<MigrateToMongoCommandHandler> Logger)
        {
            this.sqlDb = sqlDb;
            mongoClient = MongoClient;
            this.mongoSettings = mongoSettings;
            logger = Logger;
        }

        public async Task<MigrateToMongoResult> MigrateFromMongoToSql()
        {
            var database = mongoClient.GetDatabase(mongoSettings.Value.DatabaseName);
            var CustomerCollection = database.GetCollection<Customer>("Customers");
            var UsersCollection = database.GetCollection<MongoUser>("Users");
            
            var mongoCustomers= CustomerCollection.Find(_ => true).ToList();
            var mongoUsers= UsersCollection.Find(_ => true).ToList();

            int migratedCount = 0;
            int skippedCount = 0;
            List<Customer> customers = new List<Customer>();
            foreach (var customer in mongoCustomers)
            {
                var alreadyExists = await sqlDb.Customers
                    .FirstOrDefaultAsync(m => m.Phone == customer.Phone);

                if (alreadyExists!=null)
                {
                    logger.LogInformation(
                        "Skipping customer {Phone} — already exists in MongoDB",
                        customer.Phone);
                    skippedCount++;
                    continue;
                }

                var SqlCustomer = new Customer
                {
                    Id = customer.Id,
                    Name = customer.Name,
                    Phone = customer.Phone,
                    CreatedAt = customer.CreatedAt,
                    CreatedBy = customer.CreatedBy,
                    ChangedAt = customer.ChangedAt,
                    ChangedBy = customer.ChangedBy,
                    Addresses = customer.Addresses
                        .Select(a => new Address
                        {
                            AddressType = a.AddressType,
                            AddressName = a.AddressName
                        })
                        .ToList()
                };
                customers.Add(SqlCustomer);
                logger.LogInformation("Migrated customer {Name}", SqlCustomer.Name);
                migratedCount++;
            }
            sqlDb.Customers.AddRange(customers);


            List<User> Users = new List<User>();
            foreach (var user in mongoUsers)
            {
                var alreadyExists = await sqlDb.Users
                    .FirstOrDefaultAsync(m => m.Email == user.Email);
                if (alreadyExists!=null)
                {
                    logger.LogInformation(
                        "Skipping User {Email} — already exists in MongoDB",
                        user.Email);
                    skippedCount++;
                    continue;
                }

                var mongoUser = new MongoUser
                {
                    Id = user.Id,
                    Email = user.Email,
                    UserName = user.UserName,
                    RefreshToken = user.RefreshToken,
                    RefreshTokenExpiryTime = user.RefreshTokenExpiryTime,
                };
                mongoUsers.Add(mongoUser);
                logger.LogInformation("Migrated customer {Name}", mongoUser.UserName);
                migratedCount++;
            }
            await sqlDb.Users.AddRangeAsync(Users);
            await sqlDb.SaveChangesAsync();

            logger.LogInformation(
                "Migration complete — Migrated: {Migrated}, Skipped: {Skipped}",
                migratedCount, skippedCount);

            return new MigrateToMongoResult
            {
                MigratedCount = migratedCount,
                SkippedCount = skippedCount
            };

        }

        public async Task<MigrateToMongoResult> MigrateFromSqlToMongo()
        {
            var database = mongoClient.GetDatabase(mongoSettings.Value.DatabaseName);
            var mongoCollection = database.GetCollection<Customer>("Customers");
            var mongoUsersCollection = database.GetCollection<MongoUser>("Users");

            var sqlCustomers = await sqlDb.Customers
                .Include(c => c.Addresses)          
                .AsNoTracking()
                .ToListAsync();

            var sqlUsers = await sqlDb.Users
                .AsNoTracking()
                .ToListAsync();

            int migratedCount = 0;
            int skippedCount = 0;

            foreach (var sqlCustomer in sqlCustomers)
            {
                var alreadyExists = await mongoCollection
                    .Find(m => m.Phone == sqlCustomer.Phone)
                    .AnyAsync();

                if (alreadyExists)
                {
                    logger.LogInformation(
                        "Skipping customer {Phone} — already exists in MongoDB",
                        sqlCustomer.Phone);
                    skippedCount++;
                    continue;
                }

                var mongoCustomer = new Customer
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    Name = sqlCustomer.Name,
                    Phone = sqlCustomer.Phone,
                    CreatedAt = sqlCustomer.CreatedAt,
                    CreatedBy = sqlCustomer.CreatedBy,
                    ChangedAt = sqlCustomer.ChangedAt,
                    ChangedBy = sqlCustomer.ChangedBy,
                    Addresses = sqlCustomer.Addresses
                        .Select(a => new Address
                        {
                            AddressType = a.AddressType,
                            AddressName = a.AddressName
                        })
                        .ToList()
                };

                await mongoCollection.InsertOneAsync(mongoCustomer);
                logger.LogInformation("Migrated customer {Name}", mongoCustomer.Name);
                migratedCount++;
            }

            List<MongoUser> mongoUsers = new List<MongoUser>();
            foreach (var sqlUser in sqlUsers)
            {
                var alreadyExists = await mongoUsersCollection
                    .Find(m => m.Email == sqlUser.Email)
                    .AnyAsync();

                if (alreadyExists)
                {
                    logger.LogInformation(
                        "Skipping User {Email} — already exists in MongoDB",
                        sqlUser.Email);
                    skippedCount++;
                    continue;
                }

                var mongoUser = new MongoUser
                {
                    Id = sqlUser.Id,
                    Email =     sqlUser.Email,
                    UserName = sqlUser.UserName,
                    RefreshToken = sqlUser.RefreshToken,
                    RefreshTokenExpiryTime = sqlUser.RefreshTokenExpiryTime,
                };
                mongoUsers.Add(mongoUser);
                logger.LogInformation("Migrated customer {Name}", mongoUser.UserName);
                migratedCount++;
            }
            await mongoUsersCollection.InsertManyAsync(mongoUsers);

            logger.LogInformation(
                "Migration complete — Migrated: {Migrated}, Skipped: {Skipped}",
                migratedCount, skippedCount);

            return new MigrateToMongoResult
            {
                MigratedCount = migratedCount,
                SkippedCount = skippedCount
            };
        }
    }   
}
