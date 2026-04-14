using AuthServer.Setting;
using Microsoft.Extensions.Options;

namespace AuthServer.Services
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
                throw new Exception($"Tenant with id {tenantId} not found");
            }
        }
        public Tenant? GetCurrentTenant()
        {
            return _currentTenant;
        }
    }
}
