using CustomersTask4.Abstraction;
using CustomersTask4.Domain;
using CustomersTask4.DTO;
using CustomersTask4.UserHandler.Command;
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
        public async Task<IActionResult> Login(LoginDto request)
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
            var refreshToken = GenerateRefreshToken();
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
            var role = userManager.GetRolesAsync(user)?.Result.FirstOrDefault() ?? "no role";

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

        [HttpPost("refresh/me")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.RefreshToken))
                return Unauthorized(new { error = "Refresh token is required" });

            var principal = GetPrincipalFromExpiredToken(request.AccessToken);
            if (principal == null)
                return Unauthorized(new { error = "Invalid access token" });

            var email = principal.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
                return Unauthorized(new { error = "Invalid token claims" });

            var user = await userManager.FindByEmailAsync(email);
            if (user == null || user.RefreshToken != request.RefreshToken)
                return Unauthorized(new { error = "Invalid refresh token" });

            if (user.RefreshTokenExpiryTime < DateTime.UtcNow)
                return Unauthorized(new { error = "Refresh token has expired" });

            var newAccessToken = GenerateJwtToken(user);
            var newRefreshToken = GenerateRefreshToken();

            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(1);
            await userManager.UpdateAsync(user);

            return Ok(new
            {
                tokenType = "Bearer",
                accessToken = newAccessToken,
                refreshToken = newRefreshToken,
                expiresIn = 3600
            });
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

        private ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
        {
            try
            {
                var secretKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes("this is my secret key abdo saad key"));

                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience = false,
                    ValidateIssuer = false,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = secretKey,
                    ValidateLifetime = false // Don't validate lifetime for expired tokens
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);
                if (!(securityToken is JwtSecurityToken jwtSecurityToken) ||
                      !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    return null;
                }

                return principal;
            }
            catch
            {
                return null;
            }
        }
    }
}
