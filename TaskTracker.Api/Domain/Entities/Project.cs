namespace TaskTracker.Api.Domain.Entities;

public class Project
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Code { get; set; } = null!;

    public DateTime CreatedAtUtc { get; set; }

    public List<TaskItem> Tasks { get; set; } = new();
}