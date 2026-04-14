namespace AuthServer.Handlers.RegisterHandler
{
    public class RegisterUserCommand
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
