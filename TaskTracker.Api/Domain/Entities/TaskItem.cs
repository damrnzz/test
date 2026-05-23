namespace TaskTracker.Api.Domain.Entities;

using TaskStatus = Enums.TaskStatus;

public class TaskItem
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public TaskStatus Status { get; set; }

    public int Priority { get; set; }

    public DateTime? DueDateUtc { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    public int CreatedByUserId { get; set; }
    public User CreatedByUser { get; set; } = null!;

    public List<TaskAttachment> Attachments { get; set; } = [];
}