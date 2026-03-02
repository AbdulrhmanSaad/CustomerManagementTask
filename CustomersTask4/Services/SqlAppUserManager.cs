using CustomersTask4.Domain;
using Microsoft.AspNetCore.Identity;

namespace CustomersTask4.Services
{
    public class SqlAppUserManager(UserManager<User> userManager,RoleManager<IdentityRole>roleManager) : IAppUserManager
    {
        public async Task<IAppUser?> FindByEmailAsync(string email)
            => await userManager.FindByEmailAsync(email);

        public async Task<bool> CheckPasswordAsync(IAppUser user, string password)
            => await userManager.CheckPasswordAsync((User)user, password);

        public async Task<IEnumerable<string>> GetRolesAsync(IAppUser user)
            => await userManager.GetRolesAsync((User)user);

        public async Task UpdateAsync(IAppUser user)
            => await userManager.UpdateAsync((User)user);

        public async Task<bool> CreateAsync(IAppUser user, string password)
        {
            var result = await userManager.CreateAsync((User)user, password);
            return result.Succeeded;
        }

        public async Task AddToRoleAsync(IAppUser user, string role)
            => await userManager.AddToRoleAsync((User)user, role);

        public async Task<IAppUser?> FindByIdAsync(string id)
            => await userManager.FindByIdAsync(id);

        public IAppUser CreateUser(string email)
            => new User { UserName = email, Email = email };

        public Task<bool> RoleExistsAsync(string role)
           => roleManager.RoleExistsAsync(role);
    }
}