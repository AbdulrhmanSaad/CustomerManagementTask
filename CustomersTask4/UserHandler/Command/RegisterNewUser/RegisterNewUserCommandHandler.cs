using CustomersTask4.Exceptions;
using CustomersTask4.Services;
using CustomersTask4.Users;
using Mediator;

namespace CustomersTask4.UserHandler.Command
{
    public class RegisterNewUserCommandHandler(IAppUserManager userManager)
        : IRequestHandler<RegisterNewUserCommand>
    {
        public async ValueTask<Unit> Handle(RegisterNewUserCommand request, CancellationToken cancellationToken)
        {
            var user = userManager.CreateUser(request.Email);

            var succeeded = await userManager.CreateAsync(user, request.Password);

            if (!succeeded)
                throw new NotFoundException("Username Already Exists");

            await userManager.AddToRoleAsync(user, UserRoles.User);

            return Unit.Value;
        }
    }
}
