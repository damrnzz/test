namespace TaskTracker.Api.Dtos.Tasks;
using TaskStatus = TaskTracker.Api.Domain.Enums.TaskStatus;

public class UpdateTaskStatusRequest
{
    public TaskStatus Status { get; set; }
}