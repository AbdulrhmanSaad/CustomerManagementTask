using AuthServer.Domain;
using AuthServer.DTO;
using AuthServer.ResultModle;
using Microsoft.AspNetCore.Identity;

namespace AuthServer.Handlers.RegisterHandler
{
    public class RegisterUserCommandHandler(UserManager<User> _userManager,ILogger<RegisterUserCommandHandler>logger)
    {
        public async Task<Result> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            var user = new User
            {
                UserName = request.Username,
                Email = request.Email
            };

            var result = await _userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
            {
                logger.LogInformation("User creation failed: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
                return Result.Failure(result.Errors.Select(e=>e.Description));
            }
            await _userManager.AddToRoleAsync(user, UserRoles.User);
            logger.LogInformation("User created successfully!");
            return Result.Success();
        }

    }
}
