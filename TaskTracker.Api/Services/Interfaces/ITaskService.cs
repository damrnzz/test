using TaskTracker.Api.Dtos.Tasks;

namespace TaskTracker.Api.Services.Interfaces;
using TaskStatus = Domain.Enums.TaskStatus;

public interface ITaskService
{
    Task<List<TaskResponse>> GetAllAsync(CancellationToken cancellationToken);
    Task<TaskResponse?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<List<TaskResponse>> GetByStatusAsync(TaskStatus status, CancellationToken cancellationToken);
    Task<TaskResponse> CreateAsync(CreateTaskRequest request, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken);
}