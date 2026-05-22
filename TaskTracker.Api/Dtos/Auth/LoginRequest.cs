namespace TaskTracker.Api.Dtos.Auth;

public class LoginRequest
{
    public string Login { get; set; } = null!;
    public string Password { get; set; } = null!;
}