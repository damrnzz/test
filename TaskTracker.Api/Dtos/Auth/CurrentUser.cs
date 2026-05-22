namespace TaskTracker.Api.Dtos.Auth;

public class CurrentUser
{
    public int Id { get; set; }
    public string Login { get; set; } = null!;
    public string Role { get; set; } = null!;
    public string Department { get; set; } = null!;
    public bool IsAuthenticated { get; set; }
}