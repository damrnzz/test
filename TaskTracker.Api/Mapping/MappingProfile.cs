using AutoMapper;
using TaskTracker.Api.Domain.Entities;
using TaskTracker.Api.Dtos.Attachments;
using TaskTracker.Api.Dtos.Projects;
using TaskTracker.Api.Dtos.Tasks;

namespace TaskTracker.Api.Mapping;

public class MappingProfile: Profile
{
    public MappingProfile()
    {
        CreateMap<Project, ProjectResponse>();

        CreateMap<TaskItem, TaskResponse>()
            .ForMember(dest => dest.ProjectName, opt => opt.MapFrom(src => src.Project.Name))
            .ForMember(dest => dest.CreatedByLogin, opt => opt.MapFrom(src => src.CreatedByUser.Login));
        
        CreateMap<TaskAttachment, AttachmentResponse>()
            .ForMember(dest => dest.UploadedByUserName,
                opt => opt.MapFrom(src => src.UploadedByUser.Login));
    }
}