namespace TaskTracker.Api.Dtos.Auth;

public class RegisterUserRequest
{
    public string Login { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string Department { get; set; } = null!;
}