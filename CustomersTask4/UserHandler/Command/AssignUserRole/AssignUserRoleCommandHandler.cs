using CustomersTask4.Domain;
using CustomersTask4.Exceptions;
using CustomersTask4.Services;
using Mediator;
using Microsoft.AspNetCore.Identity;

namespace CustomersTask4.UserHandler.Command.AssignUserRole
{
    public class AssignUserRoleCommandHandler
        (ILogger<AssignUserRoleCommandHandler> logger,
        IAppUserManager userManager
        ) : IRequestHandler<AssignUserRoleCommand,Unit>
    {
        public async ValueTask<Unit> Handle(AssignUserRoleCommand request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Assigning role {RoleName} to user with email {Email}", request.RoleName, request.Email);

            var user=await userManager.FindByEmailAsync(request.Email);
            if (user == null) 
                throw new NotFoundException("User Not Found");

            var roleExists=await userManager.RoleExistsAsync(request.RoleName);
            if (!roleExists)
                throw new NotFoundException("Role Not Found");

            await userManager.AddToRoleAsync(user, request.RoleName);
            return Unit.Value;
        }
    }
}
