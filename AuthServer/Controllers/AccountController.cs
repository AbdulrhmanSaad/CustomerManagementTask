using AuthServer.DTO;
using AuthServer.Handlers.PasswordGrand;
using AuthServer.Handlers.RefreshTokenGrand;
using AuthServer.Handlers.RegisterHandler;
using AuthServer.ResultModle;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using Shared.Services;
using Wolverine;

namespace AuthServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableRateLimiting("fixed")]
    public class AccountController(
        IMessageBus _meditor,
        ILocalizationService localization) : ControllerBase
    {
        [HttpPost]
        [Route("register")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Register(RegisterUserCommand command)
        {

           var result=await _meditor.InvokeAsync<Result>(command);                     
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }
            return Ok(localization.Localize("User created successfully"));
        }


        [HttpPost("token"), IgnoreAntiforgeryToken]
        [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Exchange()
        {
            var request = HttpContext.GetOpenIddictServerRequest();
            if (request == null)
                throw new InvalidOperationException(localization.Localize("The OpenIddict request cannot be retrieved."));

            if (request.IsPasswordGrantType())
            {
                var command = new PasswordGrantCommand {
                 UserName = request.Username!,
                 Password = request.Password!,
                 Scopes = request.GetScopes()
             };

                var result = await _meditor.InvokeAsync<AuthResult>(command);

                if (!result.IsSuccess)
                    return BadRequest(result.ErrorMessage);

                return SignIn(result.Principal!, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            }

            if (request.IsRefreshTokenGrantType())
            {
                var authResult = await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

                var command = new RefreshTokenGrantCommand
                {
                    Principal = authResult.Principal!,
                    Scopes = request.GetScopes()
                };

                var result = await _meditor.InvokeAsync<AuthResult>(command);

                if (!result.IsSuccess)
                    return BadRequest(result.ErrorMessage);

                return SignIn(result.Principal!, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            }

            return BadRequest(new { error = localization.Localize("unsupported_grant_type") });
        }
    }
}
