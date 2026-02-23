using CustomersTask4.Domain;
using CustomersTask4.UserHandler.Command;
using CustomersTask4.Users;
using MediatR;
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
    public class IdentityController(IMediator mediator, UserManager<User> userManager, SignInManager<User> signInManager) : ControllerBase
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
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = new User { UserName = request.Email, Email = request.Email };

            var result = await userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
                return BadRequest(new { errors = result.Errors.Select(e => e.Description) });

            // Assign default role to new user
            await userManager.AddToRoleAsync(user, UserRoles.User);

            return Ok(new { message = "User registered successfully" });
        }

        [HttpPost("login/me")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var user = await userManager.FindByEmailAsync(request.Email);

            if (user == null)
                return Unauthorized(new { error = "Invalid email or password" });

            var result = await signInManager.CheckPasswordSignInAsync(user, request.Password, false);

            if (!result.Succeeded)
                return Unauthorized(new { error = "Invalid email or password" });

            var accessToken = GenerateJwtToken(user);
            var refreshToken = GenerateJwtToken(user);
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(1);

            await userManager.UpdateAsync(user);
            await signInManager.SignInAsync(user, false);


            return Ok(new
            {
                tokenType = "Bearer",
                accessToken = accessToken,
                expiresIn = 3600,
                refreshToken = refreshToken
            });
        }
        private string GenerateJwtToken(User user)
        {
            var role =userManager.GetRolesAsync(user)?.Result.FirstOrDefault()??"no role";

            var claim = new List<Claim>();
            claim.Add(new Claim(ClaimTypes.Name, user.UserName));
            claim.Add(new Claim(ClaimTypes.Role, role));
            claim.Add(new Claim(ClaimTypes.NameIdentifier, user.Id));

            var secretKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes("this is my secret key abdo saad key"));
            var signinCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: claim,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: signinCredentials
                );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            return tokenString;
        }
        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }
    }
    }
