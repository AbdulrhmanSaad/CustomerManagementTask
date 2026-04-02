using CustomersTask4.Domain;
using Microsoft.AspNetCore.Identity;

namespace CustomersTask4.Domain
{
    public class User : IdentityUser, IAppUser, IMustHaveTenant
    {
        public string? RefreshToken { get; set; }
        public DateTime RefreshTokenExpiryTime { get; set; }
        public string TenantId { get; set; } = null!;
    }
}
