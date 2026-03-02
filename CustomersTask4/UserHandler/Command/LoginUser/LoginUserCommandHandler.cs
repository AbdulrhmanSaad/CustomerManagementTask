using CustomersTask4.Domain;
using CustomersTask4.DTO;
using CustomersTask4.Exceptions;
using CustomersTask4.Services;
using Mediator;
using Microsoft.AspNetCore.Identity;

namespace CustomersTask4.UserHandler.Command.LoginUser
{
    public class LoginUserCommandHandler(
        IAppUserManager userManager,
        IUserTokenMangerService userTokenManger)
        : IRequestHandler<LoginUserCommand, LoginDto>
    {
        public async ValueTask<LoginDto> Handle(LoginUserCommand request, CancellationToken cancellationToken)
        {
            var user = await userManager.FindByEmailAsync(request.Email);

            if (user == null)
                throw new NotFoundException("Invalid Email Or Password");

            var isValid = await userManager.CheckPasswordAsync(user, request.Password);

            if (!isValid)
                throw new NotFoundException("Invalid Email Or Password");

            var roles = await userManager.GetRolesAsync(user);
            var accessToken = userTokenManger.GenerateJwtToken(user, roles);
            var refreshToken = userTokenManger.GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(1);

            await userManager.UpdateAsync(user);

            return new LoginDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
            };
        }
    }
}
