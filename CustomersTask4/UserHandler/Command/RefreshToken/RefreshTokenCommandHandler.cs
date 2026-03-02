using CustomersTask4.DTO;
using CustomersTask4.Exceptions;
using CustomersTask4.Services;
using Mediator;
using System.Security.Claims;

namespace CustomersTask4.UserHandler.Command.RefreshToken
{
    public class RefreshTokenCommandHandler(
        IAppUserManager userManager,
        IUserTokenMangerService userTokenManger)
        : IRequestHandler<RefreshTokenCommand, LoginDto>
    {
        public async ValueTask<LoginDto> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            var principal = userTokenManger.GetPrincipalFromExpiredToken(request.AccessToken);
            if (principal == null)
                throw new NotFoundException("Invalid access token");

            var email = principal.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
                throw new NotFoundException("Invalid token claim");

            var user = await userManager.FindByEmailAsync(email);
            if (user == null || user.RefreshToken != request.RefreshToken)
                throw new NotFoundException("Invalid refresh token");

            if (user.RefreshTokenExpiryTime < DateTime.UtcNow)
                throw new NotFoundException("Refresh token has expired");

            var roles = await userManager.GetRolesAsync(user);
            var newAccessToken = userTokenManger.GenerateJwtToken(user, roles);
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
