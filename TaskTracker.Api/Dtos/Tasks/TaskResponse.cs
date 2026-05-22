namespace TaskTracker.Api.Dtos.Tasks;

public class TaskResponse
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public TaskStatus Status { get; set; }
    public int Priority { get; set; }
    public DateTime? DueDateUtc { get; set; }
    public int ProjectId { get; set; }
    public string ProjectName { get; set; } = null!;
    public int CreatedByUserId { get; set; }
    public string CreatedByLogin { get; set; } = null!;
}