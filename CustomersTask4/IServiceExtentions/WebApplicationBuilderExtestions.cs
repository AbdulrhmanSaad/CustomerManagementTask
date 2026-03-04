using AspNetCore.Identity.MongoDbCore.Extensions;
using AspNetCore.Identity.MongoDbCore.Infrastructure;
using CustomersTask4.Data;
using CustomersTask4.Domain;
using CustomersTask4.Repository;
using CustomersTask4.Services;
using CustomersTask4.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;

namespace CustomersTask4.IServiceExtentions
{
    public static class WebApplicationBuilderExtestions
    {
        public static void AddMongoSetings(this WebApplicationBuilder builder)
        {
            builder.Services.Configure<MongoDbSetting>(
                builder.Configuration.GetSection("MongoDbSetting"));

            builder.Services.AddSingleton<IMongoClient>(sp =>
            {
                var settings = builder.Configuration
                    .GetSection("MongoDbSetting")
                    .Get<MongoDbSetting>();
                return new MongoClient(settings?.ConnectionString);
            });

            var mongoSettings = builder.Configuration
                .GetSection("MongoDbSetting")
                .Get<MongoDbSetting>();

            var mongoIdentityConfig = new MongoDbIdentityConfiguration
            {
                MongoDbSettings = new MongoDbSettings
                {
                    ConnectionString = mongoSettings!.ConnectionString,
                    DatabaseName = mongoSettings.DatabaseName
                },
                IdentityOptionsAction = options =>
                {
                    options.Password.RequireDigit = false;
                    options.Password.RequiredLength = 6;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequireUppercase = false;
                }
            };

            builder.Services.ConfigureMongoDbIdentity<MongoUser, MongoRole, string>(mongoIdentityConfig)
                .AddUserManager<UserManager<MongoUser>>()
                .AddRoleManager<RoleManager<MongoRole>>()
                .AddSignInManager<SignInManager<MongoUser>>()
                .AddDefaultTokenProviders();

            builder.Services.AddScoped<IGenericRepository<Customer>, MongoCustomerRepository>();
            builder.Services.AddScoped<ICustomerHistoryRepository, MongoCustomerRepository>();

            builder.Services.AddScoped<IAppUserManager, MongoAppUserManager>();
        }

        public static void AddSqlSetings(this WebApplicationBuilder builder)
        {
            

            builder.Services.AddScoped(
                typeof(IGenericRepository<>),
                typeof(GenericRepository<>));

            builder.Services.AddScoped<ICustomerHistoryRepository, CustomerHistoryRepository>();
            builder.Services.AddScoped<IAppUserManager, SqlAppUserManager>();

        }




        public static async Task SeedMongoRolesAsync(this IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<MongoRole>>();

            string[] roles = [UserRoles.Admin, UserRoles.User];

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new MongoRole { Name = role });
            }
        }
    }
}

