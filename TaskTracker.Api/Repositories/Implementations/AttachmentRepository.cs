using Microsoft.EntityFrameworkCore;
using TaskTracker.Api.Data;
using TaskTracker.Api.Domain.Entities;
using TaskTracker.Api.Repositories.Interfaces;

namespace TaskTracker.Api.Repositories.Implementations;

public class AttachmentRepository : IAttachmentRepository
{
    private readonly AppDbContext _context;

    public AttachmentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<TaskAttachment?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        return await _context.TaskAttachments.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task<TaskAttachment?> GetByTaskAndAttachmentIdAsync(int taskId, int attachmentId, CancellationToken cancellationToken)
    {
        return _context.TaskAttachments.Include(x => x.UploadedByUser)
            .FirstOrDefaultAsync(x => x.Id == attachmentId && x.TaskItemId == taskId, cancellationToken);
    }

    public async Task<List<TaskAttachment>> GetByTaskIdAsync(int taskId, CancellationToken cancellationToken)
    {
        return await _context.TaskAttachments.Include(x => x.UploadedByUser)
            .Where(x => x.TaskItemId == taskId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(TaskAttachment attachment, CancellationToken cancellationToken)
    {
        await _context.TaskAttachments.AddAsync(attachment, cancellationToken);
    }

    public Task DeleteAsync(TaskAttachment attachment, CancellationToken cancellationToken)
    {
        _context.TaskAttachments.Remove(attachment);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}