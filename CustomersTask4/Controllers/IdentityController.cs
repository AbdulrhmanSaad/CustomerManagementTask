using CustomersTask4.Abstraction;
using CustomersTask4.Domain;
using CustomersTask4.DTO;
using CustomersTask4.UserHandler.Command;
using CustomersTask4.UserHandler.Command.LoginUser;
using CustomersTask4.UserHandler.Command.RefreshToken;
using CustomersTask4.Users;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CustomersTask4.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class IdentityController(IAppMeditor mediator, UserManager<User> userManager, SignInManager<User> signInManager) : ControllerBase
    {
        [HttpPost("AssignRoleTo")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<ActionResult> AddRoleToUser(AssignUserRoleCommand command)
        {
            await mediator.Send(command);
            return NoContent();
        }


        [HttpPost("register/me")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register(RegisterNewUserCommand request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await mediator.Send(request);
            return Ok(new { message = "User registered successfully" });
        }

        [HttpPost("login/me")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Login(LoginUserCommand request)
        {
           var token= await mediator.Send(request);
            return Ok(token);
        }

        [HttpPost("refresh/me")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenCommand request)
        {
            if (string.IsNullOrWhiteSpace(request.RefreshToken))
                return Unauthorized(new { error = "Refresh token is required" });

            var token=await mediator.Send(request);
            return Ok(token);
        }
       
        
    }
}
