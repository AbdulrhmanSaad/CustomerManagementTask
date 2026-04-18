using AuthServer.Domain;
using AuthServer.ResultModle;
using AuthServer.Services;
using Microsoft.AspNetCore.Identity;
using OpenIddict.Abstractions;
using Shared.Services;

namespace AuthServer.Handlers.PasswordGrand
{
    public class PasswordGrantHandler(UserManager<User> _userManager,
        ITenantService _tenantService,
        ILocalizationService _localization,
        IPrincipalFactory _principalFactory)
    {
        public async Task<AuthResult> Handle(PasswordGrantCommand command)
        {
            var user = await _userManager.FindByNameAsync(command.UserName);

            if (user == null || !await _userManager.CheckPasswordAsync(user, command.Password))
                return AuthResult.Failure(_localization.Localize("Invalid User Name OR Password"));

            var currentTenant = _tenantService.GetCurrentTenant()!.TenantId;

            if (currentTenant != user.TenantId)
                return AuthResult.Failure(_localization.Localize("Invalid tenant for the user:{0}", command.UserName));

            var principal = await _principalFactory.CreatePrincipal(user, currentTenant);

            principal.SetScopes(command.Scopes);
            principal.SetResources("resource-server");
            principal.SetDestinations(_ => new[]
            {
            OpenIddictConstants.Destinations.AccessToken,
            OpenIddictConstants.Destinations.IdentityToken
        });

            return AuthResult.Success(principal);
        }
    }
}
