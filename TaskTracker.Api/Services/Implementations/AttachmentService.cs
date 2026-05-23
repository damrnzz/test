using AutoMapper;
using FluentValidation;
using TaskTracker.Api.Domain.Entities;
using TaskTracker.Api.Dtos.Attachments;
using TaskTracker.Api.Dtos.Auth;
using TaskTracker.Api.Repositories.Interfaces;
using TaskTracker.Api.Services.Interfaces;

namespace TaskTracker.Api.Services.Implementations;

public class AttachmentService : IAttachmentService
{
    private readonly IAttachmentRepository _attachmentRepository;
    private readonly ITaskRepository _taskRepository;
    private readonly ICurrentUserAccessor _currentUserAccessor;
    private readonly IMapper _mapper;
    private readonly ILogger<AttachmentService> _logger;

    public AttachmentService(
        IAttachmentRepository attachmentRepository,
        ITaskRepository taskRepository,
        ICurrentUserAccessor currentUserAccessor,
        IMapper mapper,
        IValidator<CreateAttachmentRequest> validator,
        ILogger<AttachmentService> logger)
    {
        _attachmentRepository = attachmentRepository;
        _taskRepository = taskRepository;
        _currentUserAccessor = currentUserAccessor;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<List<AttachmentResponse>> GetTaskAttachmentsAsync(int taskId, CancellationToken cancellationToken)
    {
        var task = await _taskRepository.GetByIdAsync(taskId, cancellationToken);
        if (task is null)
            throw new InvalidOperationException($"Task not found.");

        var attachments = await _attachmentRepository.GetByTaskIdAsync(taskId, cancellationToken);
        return _mapper.Map<List<AttachmentResponse>>(attachments);
    }

    public async Task<AttachmentResponse> AddAttachmentAsync(int taskId, CreateAttachmentRequest request, CancellationToken cancellationToken)
    {
        var task = await _taskRepository.GetByIdAsync(taskId, cancellationToken);
        if (task is null)
            throw new InvalidOperationException($"Task not found.");
        
        var currentUser = _currentUserAccessor.GetCurrentUser();
        if (!currentUser.IsAuthenticated)
            throw new InvalidOperationException("User is not authenticated.");

        var attachment = new TaskAttachment
        {
            TaskItemId = taskId,
            UploadedByUserId = currentUser.Id,
            FileName = request.FileName,
            Url = request.Url,
            SizeBytes = request.SizeBytes,
            CreatedAtUtc = DateTime.UtcNow
        };

        await _attachmentRepository.AddAsync(attachment, cancellationToken);
        await _attachmentRepository.SaveChangesAsync(cancellationToken);
        
        var createdAttachment = await _attachmentRepository.GetByIdAsync(attachment.Id, cancellationToken);
        return _mapper.Map<AttachmentResponse>(createdAttachment);
    }

    public async Task<bool> DeleteAttachmentAsync(int taskId, int attachmentId, CancellationToken cancellationToken)
    {
        var task = await _taskRepository.GetByIdAsync(taskId, cancellationToken);
        if (task is null)
            throw new InvalidOperationException($"Task not found.");
        
        var attachment = await _attachmentRepository.GetByTaskAndAttachmentIdAsync(taskId, attachmentId, cancellationToken);
        if (attachment is null)
            return false;
        
        var currentUser = _currentUserAccessor.GetCurrentUser();
        if (!currentUser.IsAuthenticated)
            throw new InvalidOperationException("User is not authenticated.");
        
        var canDelete = attachment.UploadedByUserId == currentUser.Id ||
                        currentUser.Role == "Manager" ||
                        currentUser.Role == "Admin";

        if (!canDelete)
            throw new InvalidOperationException("");

        await _attachmentRepository.DeleteAsync(attachment, cancellationToken);
        await _attachmentRepository.SaveChangesAsync(cancellationToken);

        return true;
    }
}