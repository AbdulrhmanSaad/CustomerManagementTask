using CustomersTask4.Domain;
using CustomersTask4.Exceptions;
using CustomersTask4.Users;
using Mediator;
using Microsoft.AspNetCore.Identity;

namespace CustomersTask4.UserHandler.Command
{
    public class RegisterNewUserCommandHandler(UserManager<User> userManager) : IRequestHandler<RegisterNewUserCommand>
    {
        public async ValueTask<Unit> Handle(RegisterNewUserCommand request, CancellationToken cancellationToken)
        {
            var user = new User { UserName = request.Email, Email = request.Email };

            var result = await userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
                throw new  NotFoundException($"Username Already Exists");

            await userManager.AddToRoleAsync(user, UserRoles.User);

            return Unit.Value;

        }
    }
}
