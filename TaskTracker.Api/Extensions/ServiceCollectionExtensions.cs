using TaskTracker.Api.Auth;
using TaskTracker.Api.Clients;
using TaskTracker.Api.Data;
using TaskTracker.Api.Mapping;
using TaskTracker.Api.Options;
using TaskTracker.Api.Repositories.Implementations;
using TaskTracker.Api.Repositories.Interfaces;
using TaskTracker.Api.Services.Implementations;
using TaskTracker.Api.Services.Interfaces;
using TaskTracker.Api.Validators;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TaskTracker.Api.Dtos.Auth;

namespace TaskTracker.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseNpgsql(configuration["Database:ConnectionString"]);
            options.UseSnakeCaseNamingConvention();
        });

        services.AddAutoMapper(expression =>
        {
            expression.AddProfile<MappingProfile>();
        });

        services.AddValidatorsFromAssemblyContaining<CreateTaskRequestValidator>();

        services.Configure<NotificationOptions>(
            configuration.GetSection(NotificationOptions.SectionName));

        services.Configure<JwtOptions>(
            configuration.GetSection(JwtOptions.SectionName));

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IProjectRepository, ProjectRepository>();
        services.AddScoped<ITaskRepository, TaskRepository>();

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IProjectService, ProjectService>();
        services.AddScoped<ITaskService, TaskService>();
        services.AddScoped<IDeadlineNotifier, DeadlineNotifier>();

        services.AddScoped<ICurrentUserAccessor, CurrentUserAccessor>();
        services.AddSingleton<PasswordHasher>();
        services.AddSingleton<JwtTokenGenerator>();
        services.AddHttpContextAccessor();

        services.AddHttpClient<IQuoteClient, QuoteClient>(client =>
        {
            client.BaseAddress = new Uri("https://api.quotable.io");
            client.Timeout = TimeSpan.FromSeconds(5);
        });
        
        services.AddScoped<IAttachmentRepository, AttachmentRepository>();
        services.AddScoped<IAttachmentService, AttachmentService>();

        return services;
    }
}