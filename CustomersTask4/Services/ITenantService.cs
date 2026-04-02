using CustomersTask4.Setting;

namespace CustomersTask4.Services
{
    public interface ITenantService
    {
        string? GetDatabaseProvider();
        string? GetConnectionString();
        Tenant? GetCurrentTenant();
        void SetCurrentTenant(string tenantId);
    }
}
