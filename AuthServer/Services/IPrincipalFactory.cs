using AuthServer.Domain;
using System.Security.Claims;

namespace AuthServer.Services
{
    public interface IPrincipalFactory
    {
        Task<ClaimsPrincipal> CreatePrincipal(User user, string tenantId);
    }
}
