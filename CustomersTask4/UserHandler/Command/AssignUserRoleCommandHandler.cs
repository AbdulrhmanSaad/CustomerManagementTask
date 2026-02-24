using CustomersTask4.Domain;
using CustomersTask4.Exceptions;
using Mediator;
using Microsoft.AspNetCore.Identity;

namespace CustomersTask4.UserHandler.Command
{
    public class AssignUserRoleCommandHandler(ILogger<AssignUserRoleCommandHandler>logger,
        UserManager<User>userManager,
        RoleManager<IdentityRole>role) : IRequestHandler<AssignUserRoleCommand,Unit>
    {
        public async ValueTask<Unit> Handle(AssignUserRoleCommand request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Assigning role {RoleName} to user with email {Email}", request.RoleName, request.Email);

            var user=await userManager.FindByEmailAsync(request.Email);
            if (user == null) 
                throw new NotFoundException("User Not Found");

            var roleExists=await role.RoleExistsAsync(request.RoleName);
            if (!roleExists)
                throw new NotFoundException("Role Not Found");

            await userManager.AddToRoleAsync(user, request.RoleName);
            return Unit.Value;
        }
    }
}
