using CustomersTask4.Domain;
using CustomersTask4.DTO;
using CustomersTask4.Exceptions;
using CustomersTask4.Services;
using Mediator;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace CustomersTask4.UserHandler.Command.RefreshToken
{
    public class RefreshTokenCommandHandler(UserManager<User>userManager, IUserTokenMangerService userTokenManger) : IRequestHandler<RefreshTokenCommand, LoginDto>
    {
        public async ValueTask<LoginDto> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            var principal = userTokenManger.GetPrincipalFromExpiredToken(request.AccessToken);
            if (principal == null)
                throw new NotFoundException ("Invalid access token");

            var email = principal.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
                throw new NotFoundException("Invalid token Claim");

            var user = await userManager.FindByEmailAsync(email);
            if (user == null || user.RefreshToken != request.RefreshToken)
                throw new NotFoundException("Invalid refresh token");

            if (user.RefreshTokenExpiryTime < DateTime.UtcNow)
                throw new NotFoundException("Refresh token has expired");

            var newAccessToken = userTokenManger.GenerateJwtToken(user);
            var newRefreshToken = userTokenManger.GenerateRefreshToken();

            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(1);
            await userManager.UpdateAsync(user);

            return new LoginDto
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
            };
        }


        
    }
}
