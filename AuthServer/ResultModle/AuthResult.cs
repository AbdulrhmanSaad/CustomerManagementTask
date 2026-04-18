using System.Security.Claims;

namespace AuthServer.ResultModle
{
    public class AuthResult
    {
        public bool IsSuccess { get; private set; }
        public ClaimsPrincipal? Principal { get; private set; }
        public string? ErrorMessage { get; private set; }

        public static AuthResult Success(ClaimsPrincipal principal) =>
            new() { IsSuccess = true, Principal = principal };

        public static AuthResult Failure(string error) =>
            new() { IsSuccess = false, ErrorMessage = error };
    }
}
