using System.Security.Claims;

namespace AuthServer.Handlers.RefreshTokenGrand
{
    public class RefreshTokenGrantCommand
    {
        public ClaimsPrincipal Principal { get; set; }
        public IEnumerable<string> Scopes { get; set; }
    }
}
