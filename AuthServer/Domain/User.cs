using Microsoft.AspNetCore.Identity;

namespace AuthServer.Domain
{
    public class User : IdentityUser, IMustHaveTenant
    {
        public string TenantId { get; set; }
    }
}
