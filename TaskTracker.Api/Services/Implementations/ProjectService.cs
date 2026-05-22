using AutoMapper;
using TaskTracker.Api.Domain.Entities;
using TaskTracker.Api.Dtos.Projects;
using TaskTracker.Api.Repositories.Interfaces;
using TaskTracker.Api.Services.Interfaces;

namespace TaskTracker.Api.Services.Implementations;

public class ProjectService: IProjectService
{
    private readonly IProjectRepository _projectRepository;
    private readonly IMapper _mapper;

    public ProjectService(IProjectRepository projectRepository, IMapper mapper)
    {
        _projectRepository = projectRepository;
        _mapper = mapper;
    }

    public async Task<List<ProjectResponse>> GetAllAsync(CancellationToken cancellationToken)
    {
        var projects = await _projectRepository.GetAllAsync(cancellationToken);
        return _mapper.Map<List<ProjectResponse>>(projects);
    }

    public async Task<ProjectResponse> CreateAsync(CreateProjectRequest request, CancellationToken cancellationToken)
    {
        var project = new Project
        {
            Name = request.Name,
            Code = request.Code,
            CreatedAtUtc = DateTime.UtcNow
        };

        await _projectRepository.AddAsync(project, cancellationToken);
        await _projectRepository.SaveChangesAsync(cancellationToken);

        return _mapper.Map<ProjectResponse>(project);
    }
}