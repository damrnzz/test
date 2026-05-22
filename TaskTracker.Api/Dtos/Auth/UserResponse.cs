using TaskTracker.Api.Domain.Enums;

namespace TaskTracker.Api.Dtos.Auth;

public class UserResponse
{
    public int Id { get; set; }
    public string Login { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string Department { get; set; } = null!;
    public UserRole Role { get; set; }
}