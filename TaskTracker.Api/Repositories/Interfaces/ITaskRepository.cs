using TaskTracker.Api.Domain.Entities;
using TaskStatus = TaskTracker.Api.Domain.Enums.TaskStatus;

namespace TaskTracker.Api.Repositories.Interfaces;

public interface ITaskRepository
{
    Task<List<TaskItem>> GetAllAsync(CancellationToken cancellationToken);
    Task<TaskItem?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<List<TaskItem>> GetByStatusAsync(TaskStatus status, CancellationToken cancellationToken);
    Task AddAsync(TaskItem task, CancellationToken cancellationToken);
    Task DeleteAsync(TaskItem task, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}