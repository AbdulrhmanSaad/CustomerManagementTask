using CustomersTask4.Domain;
using CustomersTask4.DTO;
using CustomersTask4.Exceptions;
using CustomersTask4.Services;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace CustomersTask4.UserHandler.Command.LoginUser
{
    public class LoginUserCommandHandler(UserManager<User>userManager,
        SignInManager<User>signInManager,
        IUserTokenMangerService userTokenManger) : IRequestHandler<LoginUserCommand, LoginDto>
    {
        public async ValueTask<LoginDto> Handle(LoginUserCommand request, CancellationToken cancellationToken)
        {
            var user = await userManager.FindByEmailAsync(request.Email);

            if (user == null)
                throw new NotFoundException("Invalid Email Or Password");

            var result = await signInManager.CheckPasswordSignInAsync(user, request.Password, false);

            if (!result.Succeeded)
                throw new NotFoundException("Invalid Email Or Password");

            var accessToken = userTokenManger.GenerateJwtToken(user);
            var refreshToken = userTokenManger.GenerateRefreshToken();
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(1);

            await userManager.UpdateAsync(user);
            await signInManager.SignInAsync(user, false);
            return new LoginDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
            };
        }



       
    }
}
