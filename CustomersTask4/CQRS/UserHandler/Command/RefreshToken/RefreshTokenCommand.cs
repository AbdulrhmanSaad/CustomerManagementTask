using CustomersTask4.DTO;

namespace CustomersTask4.CQRS.UserHandler.Command.RefreshToken
{
    public class RefreshTokenCommand
    {
        public string AccessToken { get; set; } = default!;
        public string RefreshToken { get; set; } = default!;
    }
}
