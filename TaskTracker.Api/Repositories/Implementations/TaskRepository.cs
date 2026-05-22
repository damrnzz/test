using Microsoft.EntityFrameworkCore;
using TaskTracker.Api.Data;
using TaskTracker.Api.Domain.Entities;
using TaskTracker.Api.Repositories.Interfaces;
using TaskStatus = TaskTracker.Api.Domain.Enums.TaskStatus;

namespace TaskTracker.Api.Repositories.Implementations;

public class TaskRepository : ITaskRepository
{
    private readonly AppDbContext _dbContext;

    public TaskRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<List<TaskItem>> GetAllAsync(CancellationToken cancellationToken)
    {
        return _dbContext.Tasks
            .AsNoTracking()
            .Include(x => x.Project)
            .Include(x => x.CreatedByUser)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public Task<TaskItem?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        return _dbContext.Tasks
            .Include(x => x.Project)
            .Include(x => x.CreatedByUser)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task<List<TaskItem>> GetByStatusAsync(TaskStatus status, CancellationToken cancellationToken)
    {
        return _dbContext.Tasks
            .AsNoTracking()
            .Include(x => x.Project)
            .Include(x => x.CreatedByUser)
            .Where(x => x.Status == status)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(TaskItem task, CancellationToken cancellationToken)
    {
        await _dbContext.Tasks.AddAsync(task, cancellationToken);
    }

    public Task DeleteAsync(TaskItem task, CancellationToken cancellationToken)
    {
        _dbContext.Tasks.Remove(task);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}