using AuthServer.Services;
using AuthServer.Setting;

namespace AuthServer.Extentions
{
    public static class MultiTenantServiceExtentions
    {
        public static void AddMultiTenancy(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<TenantSetting>(
                configuration.GetSection("TenantSetting"));
            TenantSetting options = new();
            configuration.GetSection("TenantSetting").Bind(options);
            services.AddSingleton<ITenantService, TenantService>();
        }

    }
}
