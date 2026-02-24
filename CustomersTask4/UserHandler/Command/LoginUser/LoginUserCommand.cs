using CustomersTask4.DTO;
using Mediator;

namespace CustomersTask4.UserHandler.Command.LoginUser
{
    public class LoginUserCommand:IRequest<LoginDto>
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
