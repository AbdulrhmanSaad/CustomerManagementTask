using AuthServer.Domain;
using Microsoft.AspNetCore.Identity;
using OpenIddict.Abstractions;
using System.Security.Claims;

namespace AuthServer.Services
{
    public class PrincipalFactory(SignInManager<User> _signInManager) : IPrincipalFactory
    {
        public async Task<ClaimsPrincipal> CreatePrincipal(User user, string tenantId)
        {
            var principal = await _signInManager.CreateUserPrincipalAsync(user);

            var identity = (ClaimsIdentity)principal.Identity!;
            identity.AddClaim(new Claim(OpenIddictConstants.Claims.Subject, user.Id));
            identity.AddClaim(new Claim("tenant", tenantId));
            return principal;
        }
    }
}
