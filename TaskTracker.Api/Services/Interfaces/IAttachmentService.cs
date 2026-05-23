using TaskTracker.Api.Dtos.Attachments;

namespace TaskTracker.Api.Services.Interfaces;

public interface IAttachmentService
{
    Task<List<AttachmentResponse>> GetTaskAttachmentsAsync(int taskId, CancellationToken cancellationToken);
    Task<AttachmentResponse> AddAttachmentAsync(int taskId, CreateAttachmentRequest request, CancellationToken cancellationToken);
    Task<bool> DeleteAttachmentAsync(int taskId, int attachmentId, CancellationToken cancellationToken);
}