using CustomersTask4.Services.Caching;
using Microsoft.Extensions.Caching.Hybrid;

namespace CustomersTask4.IServiceExtentions
{
    public static class CachingExtestion
    {
        public static void AddCaching(this WebApplicationBuilder builder)
        {
            builder.Services.AddHybridCache(opt =>
            {
                opt.DefaultEntryOptions = new HybridCacheEntryOptions
                {
                    Expiration = TimeSpan.FromMinutes(10),
                    LocalCacheExpiration = TimeSpan.FromSeconds(30)
                };
            });

            builder.Services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = builder.Configuration["ConnectionString:redis"];
            });
            builder.Services.AddScoped<IRedisCachingService, RedisCachingService>();

            builder.Services.AddDistributedSqlServerCache(option =>
            {
                option.ConnectionString = builder.Configuration["ConnectionString:sqlCach"];
                option.SchemaName = "dbo";
                option.TableName = "CachEntries";
            });

            builder.AddRedisClient("redis");
        }
    }
}
