using CustomersTask4.Domain;
using CustomersTask4.Exceptions;
using CustomersTask4.Services;
using Microsoft.AspNetCore.Identity;

namespace CustomersTask4.UserHandler.Command.AssignUserRole
{
    public class AssignUserRoleCommandHandler
        (ILogger<AssignUserRoleCommandHandler> logger,
        IAppUserManager userManager,
        LocalizationService localization
        ) 
    {
        public async Task Handle(AssignUserRoleCommand request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Assigning role {RoleName} to user with email {Email}", request.RoleName, request.Email);

            var user=await userManager.FindByEmailAsync(request.Email);
            if (user == null) 
                throw new NotFoundException(localization.Localize("User Not Found"));

            var roleExists=await userManager.RoleExistsAsync(request.RoleName);
            if (!roleExists)
                throw new NotFoundException(localization.Localize("Role Not Found"));

            await userManager.AddToRoleAsync(user, request.RoleName);
        }
    }
}
