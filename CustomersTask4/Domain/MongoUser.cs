using AspNetCore.Identity.MongoDbCore.Models;
using MongoDbGenericRepository.Attributes;

namespace CustomersTask4.Domain
{
    [CollectionName("Users")]
    public class MongoUser : MongoIdentityUser<string>, IAppUser,IMustHaveTenant
    {
        public string? RefreshToken { get; set; }
        public DateTime RefreshTokenExpiryTime { get; set; }
        public string TenantId { get; set ; }
    }
}