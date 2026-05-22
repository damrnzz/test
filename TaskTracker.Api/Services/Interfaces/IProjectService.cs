using TaskTracker.Api.Dtos.Projects;

namespace TaskTracker.Api.Services.Interfaces;

public interface IProjectService
{
    Task<List<ProjectResponse>> GetAllAsync(CancellationToken cancellationToken);
    Task<ProjectResponse> CreateAsync(CreateProjectRequest request, CancellationToken cancellationToken);
}