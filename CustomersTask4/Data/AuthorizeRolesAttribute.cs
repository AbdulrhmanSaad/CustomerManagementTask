using CustomersTask4.Users;
using Microsoft.AspNetCore.Authorization;

namespace CustomersTask4.Data
{
    public class AuthorizeRolesAttribute: AuthorizeAttribute
    {
        public AuthorizeRolesAttribute(params UserRoles[] roles)
        {
            Roles = string.Join(",", roles.Select(r => ((int)r).ToString()));
        }
    }
}
