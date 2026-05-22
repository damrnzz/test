namespace TaskTracker.Api.Dtos.Projects;

public class CreateProjectRequest
{
    public string Name { get; set; } = null!;
    public string Code { get; set; } = null!;
}