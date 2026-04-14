using AuthServer.Setting;

namespace AuthServer.Services
{
    public interface ITenantService
    {
        Tenant? GetCurrentTenant();
        void SetCurrentTenant(string tenantId);
    }
}
