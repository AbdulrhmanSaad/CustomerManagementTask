using CustomersTask4.DTO;
using MediatR;

namespace CustomersTask4.UserHandler.Command.RefreshToken
{
    public class RefreshTokenCommand:IRequest<LoginDto>
    {
        public string AccessToken { get; set; } = default!;
        public string RefreshToken { get; set; } = default!;
    }
}
