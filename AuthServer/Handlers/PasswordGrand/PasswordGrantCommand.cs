namespace AuthServer.Handlers.PasswordGrand
{
    public class PasswordGrantCommand
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public IEnumerable<string> Scopes { get; set; }
        
    }
}
