using AspNetCore.Identity.MongoDbCore.Models;
using MongoDbGenericRepository.Attributes;

namespace CustomersTask4.Domain
{
    [CollectionName("Roles")]
    public class MongoRole : MongoIdentityRole<string>
    {
    }
}