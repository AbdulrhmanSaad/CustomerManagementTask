using MediatR;

namespace CustomersTask4.UserHandler.Command
{
    public class AssignUserRoleCommand:IRequest
    {
        public string Email { get; set; }
        public string RoleName { get; set; }
    }
}
