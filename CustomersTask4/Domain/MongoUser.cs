using AspNetCore.Identity.MongoDbCore.Models;
using MongoDbGenericRepository.Attributes;

namespace CustomersTask4.Domain
{
    [CollectionName("Users")]
    public class MongoUser : MongoIdentityUser<string>, IAppUser
    {
        public string? RefreshToken { get; set; }
        public DateTime RefreshTokenExpiryTime { get; set; }

     
    }
}