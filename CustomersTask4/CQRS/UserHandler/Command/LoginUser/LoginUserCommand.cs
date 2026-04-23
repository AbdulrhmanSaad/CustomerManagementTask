using CustomersTask4.DTO;

namespace CustomersTask4.CQRS.UserHandler.Command.LoginUser
{
    public class LoginUserCommand 
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
