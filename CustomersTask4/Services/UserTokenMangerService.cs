using CustomersTask4.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace CustomersTask4.Services
{
    public interface IUserTokenMangerService
    {
        string GenerateJwtToken(User user);
        string GenerateRefreshToken();
        ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);

    }
    public class UserTokenMangerService(UserManager<User>userManager):IUserTokenMangerService
    {
        public string GenerateJwtToken(User user)
        {
            var role = userManager.GetRolesAsync(user)?.Result.FirstOrDefault() ?? "no role";

            var claim = new List<Claim>();
            claim.Add(new Claim(ClaimTypes.Email, user.Email));
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

        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }

        public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
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
