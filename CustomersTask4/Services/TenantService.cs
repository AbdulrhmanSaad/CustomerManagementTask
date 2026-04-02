using CustomersTask4.Exceptions;
using CustomersTask4.Setting;
using Microsoft.Extensions.Options;

namespace CustomersTask4.Services
{
    public class TenantService : ITenantService
    {
        private Tenant? _currentTenant;
        private readonly TenantSetting _tenantsSettings;
        public TenantService(IOptions<TenantSetting> tenantsSettings)
        {
           _tenantsSettings = tenantsSettings.Value;
        }
        public void SetCurrentTenant(string tenantId)
        {
            _currentTenant = _tenantsSettings.Tenants.FirstOrDefault(t => t.TenantId == tenantId);
            if (_currentTenant is null)
            {
                throw new NotFoundException($"Tenant with id {tenantId} not found");
            }

            if (string.IsNullOrEmpty(_currentTenant.ConnectionString))
            {
                _currentTenant.ConnectionString = _tenantsSettings.Defaults.ConnectionString;
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
    }
}
