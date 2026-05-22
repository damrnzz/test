using AutoMapper;
using TaskTracker.Api.Domain.Entities;
using TaskTracker.Api.Dtos.Auth;
using TaskTracker.Api.Dtos.Tasks;
using TaskTracker.Api.Repositories.Interfaces;
using TaskTracker.Api.Services.Interfaces;
using TaskStatus = TaskTracker.Api.Domain.Enums.TaskStatus;

namespace TaskTracker.Api.Services.Implementations;

public class TaskService: ITaskService
{
    private readonly ITaskRepository _taskRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<TaskService> _logger;
    private readonly ICurrentUserAccessor _currentUserAccessor;

    public TaskService(
        ITaskRepository taskRepository,
        IProjectRepository projectRepository,
        IMapper mapper,
        ILogger<TaskService> logger,
        ICurrentUserAccessor currentUserAccessor)
    {
        _taskRepository = taskRepository;
        _projectRepository = projectRepository;
        _mapper = mapper;
        _logger = logger;
        _currentUserAccessor = currentUserAccessor;
    }

    public async Task<List<TaskResponse>> GetAllAsync(CancellationToken cancellationToken)
    {
        var tasks = await _taskRepository.GetAllAsync(cancellationToken);
        return _mapper.Map<List<TaskResponse>>(tasks);
    }

    public async Task<TaskResponse?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var task = await _taskRepository.GetByIdAsync(id, cancellationToken);
        return task is null ? null : _mapper.Map<TaskResponse>(task);
    }

    public async Task<List<TaskResponse>> GetByStatusAsync(TaskStatus status, CancellationToken cancellationToken)
    {
        var tasks = await _taskRepository.GetByStatusAsync(status, cancellationToken);
        return _mapper.Map<List<TaskResponse>>(tasks);
    }

    public async Task<TaskResponse> CreateAsync(CreateTaskRequest request, CancellationToken cancellationToken)
    {
        var currentUser = _currentUserAccessor.GetCurrentUser();
        if (!currentUser.IsAuthenticated)
            throw new InvalidOperationException("User is not authenticated.");

        var project = await _projectRepository.GetByIdAsync(request.ProjectId, cancellationToken);
        if (project is null)
            throw new InvalidOperationException("Project not found.");

        var task = new TaskItem
        {
            Title = request.Title,
            Description = request.Description,
            Priority = request.Priority,
            DueDateUtc = request.DueDateUtc,
            ProjectId = request.ProjectId,
            Status = TaskStatus.New,
            CreatedAtUtc = DateTime.UtcNow,
            CreatedByUserId = currentUser.Id
        };

        await _taskRepository.AddAsync(task, cancellationToken);
        await _taskRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Task {Title} created by user {UserId} for project {ProjectId}",
            task.Title,
            currentUser.Id,
            task.ProjectId);

        var createdTask = await _taskRepository.GetByIdAsync(task.Id, cancellationToken);
        return _mapper.Map<TaskResponse>(createdTask!);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var task = await _taskRepository.GetByIdAsync(id, cancellationToken);
        if (task is null)
            return false;

        await _taskRepository.DeleteAsync(task, cancellationToken);
        await _taskRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Task with id {TaskId} deleted", id);

        return true;
    }
}
