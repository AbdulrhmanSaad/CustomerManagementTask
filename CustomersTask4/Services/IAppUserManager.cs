using CustomersTask4.Domain;

namespace CustomersTask4.Services
{
    public interface IAppUserManager
    {
        Task<IAppUser?> FindByEmailAsync(string email);
        Task<bool> CheckPasswordAsync(IAppUser user, string password);
        Task<IEnumerable<string>> GetRolesAsync(IAppUser user);
        Task UpdateAsync(IAppUser user);
        Task<bool> CreateAsync(IAppUser user, string password);
        Task<bool> RoleExistsAsync(string role);
        Task AddToRoleAsync(IAppUser user, string role);
        Task<IAppUser?> FindByIdAsync(string id);
        IAppUser CreateUser(string email);
    }
}