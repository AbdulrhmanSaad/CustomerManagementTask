namespace CustomersTask4.CQRS.UserHandler.Command.AssignUserRole
{
    public class AssignUserRoleCommand
    {
        public string Email { get; set; }
        public string RoleName { get; set; }
    }
}
