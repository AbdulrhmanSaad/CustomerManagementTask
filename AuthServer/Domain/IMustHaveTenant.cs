namespace AuthServer.Domain
{
    public interface IMustHaveTenant
    {
        public string TenantId { get; set; }
    }
}
