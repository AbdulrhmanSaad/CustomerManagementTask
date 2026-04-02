namespace CustomersTask4.Setting
{
    public class TenantSetting
    {
        public Configration Defaults { get; set; } = default;
        public List<Tenant> Tenants { get; set; } = new();
    }
}
