using Mediator;

namespace CustomersTask4.UserHandler.Command
{
    public class RegisterNewUserCommand:IRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
