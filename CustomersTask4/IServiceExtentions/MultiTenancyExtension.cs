using CustomersTask4.Data;
using CustomersTask4.Services;
using CustomersTask4.Setting;
using Microsoft.EntityFrameworkCore;

namespace CustomersTask4.IServiceExtentions
{
    public static class MultiTenancyExtension
    {
        public static void AddMultiTenancy(this IServiceCollection services, IConfiguration configuration)
        {
            //MultiTenancy: read tenants settings from appsetting and bind it to TenantSetting class.
            services.Configure<TenantSetting>(
                configuration.GetSection("TenantSetting"));
            TenantSetting options = new();
            configuration.GetSection("TenantSetting").Bind(options);

            services.AddSingleton<ITenantService, TenantService>();
            services.AddHttpContextAccessor();

            //MultiTenancy Database Configuration
            var defualProvider = options.Defaults.DBProvider;
            if (defualProvider.ToLower() == "sql")
            {
                services.AddDbContext<ApplicationDbContext>(m => m.UseSqlServer());
                foreach (var tenant in options.Tenants)
                {
                    var connectionString = tenant.ConnectionString ?? options.Defaults.ConnectionString;

                    using var scoped = services.BuildServiceProvider().CreateScope();
                    var dbcontext = scoped.ServiceProvider.GetService<ApplicationDbContext>();

                    dbcontext?.Database.SetConnectionString(connectionString);
                    if (dbcontext.Database.GetPendingMigrations().Any())
                    {
                        dbcontext.Database.Migrate();
                    }
                }
            }
        }
    }
}
