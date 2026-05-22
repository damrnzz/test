namespace TaskTracker.Api.Dtos.Auth;

public class LoginResponse
{
    public string AccessToken { get; set; } = null!;
    public DateTime ExpiresAtUtc { get; set; }
    
    public string Login { get; set; } = null!;
    public string Role { get; set; } = null!;
}