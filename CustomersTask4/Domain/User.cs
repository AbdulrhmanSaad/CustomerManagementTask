using Microsoft.AspNetCore.Identity;

namespace CustomersTask4.Domain
{
    public class User:IdentityUser
    {
        public string? RefreshToken { get; set; }
        public DateTime RefreshTokenExpiryTime { get; set; }
    }
}
