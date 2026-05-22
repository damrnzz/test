namespace TaskTracker.Api.Dtos.Projects;

public class ProjectSummary
{
    public int ProjectId{ get; set; }
    public string ProjectName{ get; set; }
    public int TotalTasks {get;set;}
    public int NewTasksCount {get;set;}
}