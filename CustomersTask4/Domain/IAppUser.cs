namespace CustomersTask4.Domain
{
    public interface IAppUser
    {
        string Id { get; }
        string? Email { get; set; }
        string? UserName { get; set; }
        string? RefreshToken { get; set; }
        DateTime RefreshTokenExpiryTime { get; set; }
    }
}