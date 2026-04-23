using CustomersTask4.DTO;
using CustomersTask4.Exceptions;
using CustomersTask4.Services;
using Shared.Services;

namespace CustomersTask4.CQRS.UserHandler.Command.LoginUser
{
    public class LoginUserCommandHandler(
        IAppUserManager userManager,
        IUserTokenMangerService userTokenManger,
        ILocalizationService localization,
        ITenantService tenantService)
        
    {
        public async Task<LoginDto> Handle(LoginUserCommand request, CancellationToken cancellationToken)
        {
            var user = await userManager.FindByEmailAsync(request.Email);

            if (user == null)
                throw new NotFoundException(localization.Localize("Invalid Email Or Password"));

            var isValid = await userManager.CheckPasswordAsync(user, request.Password);

            if (!isValid)
                throw new NotFoundException(localization.Localize("Invalid Email Or Password"));

            var roles = await userManager.GetRolesAsync(user);
            var accessToken = userTokenManger.GenerateJwtToken(user, roles,tenantService.GetCurrentTenant().TenantId);
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
