using TaskTracker.Api.Domain.Entities;

namespace TaskTracker.Api.Repositories.Interfaces;

public interface IAttachmentRepository
{
    Task<TaskAttachment?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<TaskAttachment?> GetByTaskAndAttachmentIdAsync(int taskId, int attachmentId, CancellationToken cancellationToken);
    Task<List<TaskAttachment>> GetByTaskIdAsync(int taskId, CancellationToken cancellationToken);
    Task AddAsync(TaskAttachment attachment, CancellationToken cancellationToken);
    Task DeleteAsync(TaskAttachment attachment, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}