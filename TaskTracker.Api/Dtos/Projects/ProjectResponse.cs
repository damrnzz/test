namespace TaskTracker.Api.Dtos.Projects;

public class ProjectResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Code { get; set; } = null!;
}