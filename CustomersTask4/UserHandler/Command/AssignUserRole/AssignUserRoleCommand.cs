using Mediator;

namespace CustomersTask4.UserHandler.Command.AssignUserRole
{
    public class AssignUserRoleCommand:IRequest
    {
        public string Email { get; set; }
        public string RoleName { get; set; }
    }
}
