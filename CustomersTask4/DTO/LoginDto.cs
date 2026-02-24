namespace CustomersTask4.DTO
{
    public class LoginDto
    {
        public string tokenType { get; set; } = "Bearer"!;
        public string AccessToken { get; set; } = default!;
        public string RefreshToken { get; set; } = default!;
        public int ExpiresIn { get; set; } = 3600;
    }
}
