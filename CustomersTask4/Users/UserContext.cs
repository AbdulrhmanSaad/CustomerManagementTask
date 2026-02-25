using System.Security.Claims;

namespace CustomersTask4.Users
{
    public interface IUserContext
    {
        CurrentUser? GetCurrentUser();
    }

    public class UserContext(IHttpContextAccessor httpContextAccessor) : IUserContext
    {
        public CurrentUser? GetCurrentUser()
        {
            var user = httpContextAccessor?.HttpContext?.User;

            if (user == null)
            {
                throw new InvalidOperationException("user Context is not present");
            }
            if (user.Identity == null || !user.Identity.IsAuthenticated)
            {
                return null;
            }

            var userId = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)!.Value;
            var userName = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)!.Value;
            var userRoles = user.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList();

            var currentUser = new CurrentUser(userId, userName, userRoles);
            return currentUser;

        }
    }
}
