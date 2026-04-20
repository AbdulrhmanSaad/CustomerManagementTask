using Asp.Versioning;
using CustomersTask4.Abstraction;
using CustomersTask4.Data;
using CustomersTask4.DTO;
using CustomersTask4.UserHandler.Command;
using CustomersTask4.UserHandler.Command.AssignUserRole;
using CustomersTask4.UserHandler.Command.LoginUser;
using CustomersTask4.UserHandler.Command.RefreshToken;
using CustomersTask4.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Shared.Services;

namespace CustomersTask4.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableRateLimiting("fixed")]
    [ApiVersion("1")]
    [ApiVersion("2")]
    public class IdentityController(IAppMeditor mediator, ILocalizationService locaizer) : ControllerBase
    {
        [HttpPost("AssignRoleTo")]
        [AuthorizeRoles(UserRoles.Admin)]
        public async Task<ActionResult> AddRoleToUser(AssignUserRoleCommand command)
        {
            await mediator.Send(command);
            return NoContent();
        }

        [HttpPost("registerNew")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register(RegisterNewUserCommand request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await mediator.Send(request);
            return Ok(locaizer.Localize("User registered successfully"));
        }

        [HttpPost("loginUser")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<LoginDto>> Login(LoginUserCommand request)
        {
            var token = await mediator.Send<LoginDto>(request);
            return Ok(token);
        }

        [HttpPost("refreshUser")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenCommand request)
        {
            if (string.IsNullOrWhiteSpace(request.RefreshToken))
                return Unauthorized(new { error = locaizer.Localize("Refresh token is required") });

            var token = await mediator.Send<LoginDto>(request);
            return Ok(token);
        }
    }
}
