namespace TaskTracker.Api.Dtos.Tasks;

public class CreateTaskRequest
{
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public int Priority { get; set; }
    public DateTime? DueDateUtc { get; set; }
    public int ProjectId { get; set; }
}