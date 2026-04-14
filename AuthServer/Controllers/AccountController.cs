using AuthServer.Domain;
using AuthServer.Handlers.RegisterHandler;
using AuthServer.ResultModle;
using AuthServer.Services;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using System.Security.Claims;
using Wolverine;

namespace AuthServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController(UserManager<User> _userManager,
        SignInManager<User> _signInManager,
        ITenantService _tenantService,
        IMessageBus _meditor) : ControllerBase
    {
        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register(RegisterUserCommand command)
        {

           var result=await _meditor.InvokeAsync<Result>(command);                     
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }
            return Ok("User created successfully!");
        }


        [HttpPost("token"), IgnoreAntiforgeryToken]
        public async Task<IActionResult> Exchange()
        {
            var request = HttpContext.GetOpenIddictServerRequest();
            if (request == null)
                throw new InvalidOperationException("The OpenIddict request cannot be retrieved.");

            if (request.IsPasswordGrantType())
            {
                var user = await _userManager.FindByNameAsync(request.Username);
                if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
                {
                    return BadRequest("Invalid User Name OR Password");
                }
                var currentTenant = _tenantService.GetCurrentTenant()!.TenantId;
                if (currentTenant!=user.TenantId)
                    return BadRequest($"Invalid tenant for the user:{request.Username}");
                var principal = await CreatePrincipalAsync(user,currentTenant);
                principal.SetScopes(request.GetScopes());
                principal.SetResources("resource-server");
                principal.SetDestinations(claim => new[]
                {
                   OpenIddictConstants.Destinations.AccessToken,
                   OpenIddictConstants.Destinations.IdentityToken
                });
                return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            }

            if (request.IsRefreshTokenGrantType())
            {
                var result = await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

                var user = await _userManager.GetUserAsync(result.Principal);
                if (user == null || !await _signInManager.CanSignInAsync(user))
                {
                    return BadRequest("Invalid User Name OR Password");
                }
                var currentTenant = _tenantService.GetCurrentTenant()!.TenantId;
                if (currentTenant != user.TenantId)
                    return BadRequest($"Invalid tenant for the user.");
                var principal = await CreatePrincipalAsync(user,currentTenant);
                principal.SetScopes(request.GetScopes());
                principal.SetResources("resource-server");
                principal.SetDestinations(claim => new[]
                {
                   OpenIddictConstants.Destinations.AccessToken,
                   OpenIddictConstants.Destinations.IdentityToken
                });
                return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            }

            return BadRequest(new { error = "unsupported_grant_type" });
        }

        private async Task<ClaimsPrincipal> CreatePrincipalAsync(User user,string tenantId)
        {
            var principal = await _signInManager.CreateUserPrincipalAsync(user);

            // Set user claims
            var identity = (ClaimsIdentity)principal.Identity!;
            identity.AddClaim(new Claim(OpenIddictConstants.Claims.Subject, user.Id));
            identity.AddClaim(new Claim("tenant", tenantId));
            return principal;
        }

    }
}
