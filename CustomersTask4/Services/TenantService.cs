using CustomersTask4.Setting;
using Microsoft.Extensions.Options;

namespace CustomersTask4.Services
{
    public class TenantService : ITenantService
    {
        private readonly HttpContext? httpContext;
        private Tenant? _currentTenant;
        private readonly TenantSetting _tenantsSettings;
        public TenantService(IHttpContextAccessor httpContextAccessor, IOptions<TenantSetting> tenantsSettings)
        {
            httpContext = httpContextAccessor.HttpContext;
            _tenantsSettings = tenantsSettings.Value;

            if (httpContext is not null)
            {
                if(httpContext.Request.Headers.TryGetValue("tenant", out var tenantId))
                {
                    SetCurrentTenant(tenantId!);
                }
                else
                {
                    throw new Exception("No Tenant provided in the Request");
                }
            }
        }
        public string? GetConnectionString()
        {
           return _currentTenant is not null?
                _currentTenant.ConnectionString:
                _tenantsSettings.Defaults.ConnectionString;
        }

        public Tenant? GetCurrentTenant()
        {
            return _currentTenant;
        }

        public string? GetDatabaseProvider()
        {
            return _tenantsSettings.Defaults.DBProvider;
        }
        private void SetCurrentTenant(string tenantId)
        {
            _currentTenant = _tenantsSettings.Tenants.FirstOrDefault(t => t.TenantId == tenantId);
            if (_currentTenant is null)
            {
                throw new Exception($"Tenant with id {tenantId} not found");
            }

            if (string.IsNullOrEmpty(_currentTenant.ConnectionString))
            {
                _currentTenant.ConnectionString = _tenantsSettings.Defaults.ConnectionString;
            }
        }
    }
}
