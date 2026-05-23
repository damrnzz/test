using TaskTracker.Api.Domain.Enums;

namespace TaskTracker.Api.Domain.Entities;

public class User
{
    public int Id { get; set; }

    public string Login { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public string Department { get; set; } = null!;

    public UserRole Role { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public List<TaskItem> CreatedTasks { get; set; } = new();
    
    public List<TaskAttachment> TaskAttachments { get; set; } = new();
}
