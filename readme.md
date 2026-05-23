using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace TaskTracker.Api.Auth;

public static  class AuthExtensions
{
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtOptions = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
                         ?? throw new InvalidOperationException("Jwt settings are not configured.");

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtOptions.Issuer,

                    ValidateAudience = true,
                    ValidAudience = jwtOptions.Audience,

                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtOptions.SecretKey)),

                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });

        services.AddAuthorization(options =>
        {
            options.AddPolicy("ItDepartmentOnly", policy =>
                policy.RequireClaim("department", "IT"));
        });

        return services;
    }
}namespace TaskTracker.Api.Auth;

public class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = null!;
    public string Audience { get; set; } = null!;
    public string SecretKey { get; set; } = null!;
    public int ExpirationMinutes { get; set; }
}using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace TaskTracker.Api.Auth;

public class JwtTokenGenerator
{
    private readonly JwtOptions _jwtOptions;

    public JwtTokenGenerator(IOptions<JwtOptions> jwtOptions)
    {
        _jwtOptions = jwtOptions.Value;
    }

    public (string Token, DateTime ExpiresAtUtc) GenerateToken(long id, string login, string role, string department)
    {
        var expiresAtUtc = DateTime.UtcNow.AddMinutes(_jwtOptions.ExpirationMinutes);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, id.ToString()),
            new(ClaimTypes.Name, login),
            new(ClaimTypes.Role, role),
            new("department", department)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            expires: expiresAtUtc,
            signingCredentials: credentials);

        var tokenValue = new JwtSecurityTokenHandler().WriteToken(token);
        return (tokenValue, expiresAtUtc);
    }
}using Microsoft.AspNetCore.Identity;
using TaskTracker.Api.Domain.Entities;

namespace TaskTracker.Api.Auth;

public class PasswordHasher
{
    private readonly IPasswordHasher<User> _passwordHasher;

    public PasswordHasher()
    {
        _passwordHasher = new PasswordHasher<User>();
    }

    public string HashPassword(User user, string password)
    {
        return _passwordHasher.HashPassword(user, password);
    }

    public bool VerifyPassword(User user, string password)
    {
        var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
        return result == PasswordVerificationResult.Success;
    }
}namespace TaskTracker.Api.Clients;

public interface IQuoteClient
{
    Task<string> GetRandomQuoteAsync(CancellationToken cancellationToken);
}namespace TaskTracker.Api.Clients;

public class QuoteClient: IQuoteClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<QuoteClient> _logger;

    public QuoteClient(HttpClient httpClient, ILogger<QuoteClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<string> GetRandomQuoteAsync(CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpClient.GetAsync("/api/random", cancellationToken);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            return content;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get random quote");
            return "No quote available";
        }
    }
}using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskTracker.Api.Domain.Entities;

namespace TaskTracker.Api.Data.Configurations;

public class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.ToTable("projects");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Code)
            .IsRequired()
            .HasMaxLength(20);

        builder.HasIndex(x => x.Code)
            .IsUnique();

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();

        builder.HasMany(x => x.Tasks)
            .WithOne(x => x.Project)
            .HasForeignKey(x => x.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskTracker.Api.Domain.Entities;

namespace TaskTracker.Api.Data.Configurations;

public class TaskItemConfiguration : IEntityTypeConfiguration<TaskItem>
{
    public void Configure(EntityTypeBuilder<TaskItem> builder)
    {
        builder.ToTable("tasks");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(x => x.Description)
            .HasMaxLength(1000);

        builder.Property(x => x.Priority)
            .IsRequired();

        builder.Property(x => x.Status)
            .IsRequired();

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();

        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.ProjectId);
        builder.HasIndex(x => x.CreatedByUserId);

        builder.HasOne(x => x.Project)
            .WithMany(x => x.Tasks)
            .HasForeignKey(x => x.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.CreatedByUser)
            .WithMany(x => x.CreatedTasks)
            .HasForeignKey(x => x.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskTracker.Api.Domain.Entities;

namespace TaskTracker.Api.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Login)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Email)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.PasswordHash)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.FullName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Department)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Role)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();

        builder.HasIndex(x => x.Login).IsUnique();
        builder.HasIndex(x => x.Email).IsUnique();
    }
}using Microsoft.EntityFrameworkCore;
using TaskTracker.Api.Domain.Entities;

namespace TaskTracker.Api.Data;

public class AppDbContext : DbContext
{
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<TaskItem> Tasks => Set<TaskItem>();
    public DbSet<User> Users => Set<User>();

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}using Microsoft.AspNetCore.Identity;
using TaskTracker.Api.Auth;
using TaskTracker.Api.Domain.Entities;
using TaskTracker.Api.Domain.Enums;

namespace TaskTracker.Api.Data;

using TaskStatus = Domain.Enums.TaskStatus;

public static  class DbSeeder
{
        public static async Task SeedAsync(AppDbContext dbContext, PasswordHasher passwordHasher)
    {
        if (dbContext.Users.Any())
            return;

        var admin = new User
        {
            Login = "admin",
            Email = "admin@tasktracker.local",
            FullName = "System Administrator",
            Department = "IT",
            Role = UserRole.Admin,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };
        admin.PasswordHash = passwordHasher.HashPassword(admin, "admin123");

        var manager = new User
        {
            Login = "manager",
            Email = "manager@tasktracker.local",
            FullName = "Project Manager",
            Department = "IT",
            Role = UserRole.Manager,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };
        manager.PasswordHash = passwordHasher.HashPassword(manager, "manager123");

        var user = new User
        {
            Login = "user",
            Email = "user@tasktracker.local",
            FullName = "Regular User",
            Department = "Sales",
            Role = UserRole.User,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };
        user.PasswordHash = passwordHasher.HashPassword(user, "user123");

        var project1 = new Project
        {
            Name = "Education Platform",
            Code = "EDU",
            CreatedAtUtc = DateTime.UtcNow
        };

        var project2 = new Project
        {
            Name = "CRM System",
            Code = "CRM",
            CreatedAtUtc = DateTime.UtcNow
        };

        dbContext.Users.AddRange(admin, manager, user);
        dbContext.Projects.AddRange(project1, project2);

        dbContext.Tasks.AddRange(
            new TaskItem
            {
                Title = "Add login endpoint",
                Description = "Implement JWT login endpoint",
                Status = TaskStatus.New,
                Priority = 1,
                DueDateUtc = DateTime.UtcNow.AddDays(3),
                CreatedAtUtc = DateTime.UtcNow,
                Project = project1,
                CreatedByUser = admin
            },
            new TaskItem
            {
                Title = "Write validator for task creation",
                Description = "FluentValidation rules",
                Status = TaskStatus.InProgress,
                Priority = 2,
                DueDateUtc = DateTime.UtcNow.AddDays(2),
                CreatedAtUtc = DateTime.UtcNow,
                Project = project2,
                CreatedByUser = manager
            });

        await dbContext.SaveChangesAsync();
    }
}namespace TaskTracker.Api.Domain.Entities;

public class Project
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Code { get; set; } = null!;

    public DateTime CreatedAtUtc { get; set; }

    public List<TaskItem> Tasks { get; set; } = new();
}namespace TaskTracker.Api.Domain.Entities;

using TaskStatus = Enums.TaskStatus;

public class TaskItem
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public TaskStatus Status { get; set; }

    public int Priority { get; set; }

    public DateTime? DueDateUtc { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    public int CreatedByUserId { get; set; }
    public User CreatedByUser { get; set; } = null!;
}using TaskTracker.Api.Domain.Enums;

namespace TaskTracker.Api.Domain.Entities;

public class User
{
    public int Id { get; set; }

    public string Login { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public string Department { get; set; } = null!;

    public UserRole Role { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public List<TaskItem> CreatedTasks { get; set; } = new();
}
namespace TaskTracker.Api.Domain.Enums;

public enum TaskStatus
{
    New = 1,
    InProgress = 2,
    Done = 3,
    Cancelled = 4
}namespace TaskTracker.Api.Domain.Enums;

public enum UserRole
{
    User = 1,
    Manager = 2,
    Admin = 3
}namespace TaskTracker.Api.Dtos.Auth;

public class CurrentUser
{
    public int Id { get; set; }
    public string Login { get; set; } = null!;
    public string Role { get; set; } = null!;
    public string Department { get; set; } = null!;
    public bool IsAuthenticated { get; set; }
}using System.Security.Claims;

namespace TaskTracker.Api.Dtos.Auth;

public class CurrentUserAccessor: ICurrentUserAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserAccessor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public CurrentUser GetCurrentUser()
    {
        var user = _httpContextAccessor.HttpContext?.User;

        if (user?.Identity?.IsAuthenticated != true)
        {
            return new CurrentUser
            {
                IsAuthenticated = false
            };
        }

        var idRaw = user.FindFirstValue(ClaimTypes.NameIdentifier);

        return new CurrentUser
        {
            Id = int.TryParse(idRaw, out var id) ? id : 0,
            Login = user.FindFirstValue(ClaimTypes.Name) ?? string.Empty,
            Role = user.FindFirstValue(ClaimTypes.Role) ?? string.Empty,
            Department = user.FindFirstValue("department") ?? string.Empty,
            IsAuthenticated = true
        };
    }
}namespace TaskTracker.Api.Dtos.Auth;

public interface ICurrentUserAccessor
{
    CurrentUser GetCurrentUser();
}namespace TaskTracker.Api.Dtos.Auth;

public class LoginRequest
{
    public string Login { get; set; } = null!;
    public string Password { get; set; } = null!;
}namespace TaskTracker.Api.Dtos.Auth;

public class LoginResponse
{
    public string AccessToken { get; set; } = null!;
    public DateTime ExpiresAtUtc { get; set; }
    
    public string Login { get; set; } = null!;
    public string Role { get; set; } = null!;
}namespace TaskTracker.Api.Dtos.Auth;

public class RegisterUserRequest
{
    public string Login { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string Department { get; set; } = null!;
}using TaskTracker.Api.Domain.Enums;

namespace TaskTracker.Api.Dtos.Auth;

public class UserResponse
{
    public int Id { get; set; }
    public string Login { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string Department { get; set; } = null!;
    public UserRole Role { get; set; }
}namespace TaskTracker.Api.Dtos.Projects;

public class CreateProjectRequest
{
    public string Name { get; set; } = null!;
    public string Code { get; set; } = null!;
}namespace TaskTracker.Api.Dtos.Projects;

public class ProjectResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Code { get; set; } = null!;
}namespace TaskTracker.Api.Dtos.Tasks;

public class CreateTaskRequest
{
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public int Priority { get; set; }
    public DateTime? DueDateUtc { get; set; }
    public int ProjectId { get; set; }
}namespace TaskTracker.Api.Dtos.Tasks;

public class TaskResponse
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public TaskStatus Status { get; set; }
    public int Priority { get; set; }
    public DateTime? DueDateUtc { get; set; }
    public int ProjectId { get; set; }
    public string ProjectName { get; set; } = null!;
    public int CreatedByUserId { get; set; }
    public string CreatedByLogin { get; set; } = null!;
}namespace TaskTracker.Api.Dtos.Tasks;
using TaskStatus = TaskTracker.Api.Domain.Enums.TaskStatus;

public class UpdateTaskStatusRequest
{
    public TaskStatus Status { get; set; }
}using TaskTracker.Api.Auth;
using TaskTracker.Api.Dtos.Auth;
using TaskTracker.Api.Services.Interfaces;

namespace TaskTracker.Api.Endpoints;


public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Auth");

        group.MapPost("/register", async (
                RegisterUserRequest request,
                IAuthService authService,
                CancellationToken cancellationToken) =>
            {
                var user = await authService.RegisterAsync(request, cancellationToken);
                return Results.Created($"/api/users/{user.Id}", user);
            })
            .AllowAnonymous();

        group.MapPost("/login", async (
                LoginRequest request,
                IAuthService authService,
                CancellationToken cancellationToken) =>
            {
                var result = await authService.LoginAsync(request, cancellationToken);
                return Results.Ok(result);
            })
            .AllowAnonymous();

        return app;
    }
}using TaskTracker.Api.Dtos.Projects;
using TaskTracker.Api.Services.Interfaces;

namespace TaskTracker.Api.Endpoints;

public static  class ProjectEndpoints
{
    public static IEndpointRouteBuilder MapProjectEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/projects")
            .WithTags("Projects")
            .RequireAuthorization();

        group.MapGet("/", async (IProjectService service, CancellationToken cancellationToken) =>
        {
            var items = await service.GetAllAsync(cancellationToken);
            return Results.Ok(items);
        });

        group.MapPost("/", async (CreateProjectRequest request, IProjectService service, CancellationToken cancellationToken) =>
            {
                var created = await service.CreateAsync(request, cancellationToken);
                return Results.Created($"/api/projects/{created.Id}", created);
            })
            .RequireAuthorization(policy => policy.RequireRole("Admin", "Manager"));

        return app;
    }
}using TaskTracker.Api.Dtos.Tasks;
using TaskTracker.Api.Services.Interfaces;
using TaskStatus = TaskTracker.Api.Domain.Enums.TaskStatus;

namespace TaskTracker.Api.Endpoints;

public static class TaskEndpoints
{
    public static IEndpointRouteBuilder MapTaskEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/tasks")
            .WithTags("Tasks")
            .RequireAuthorization();

        group.MapGet("/", async (ITaskService service, CancellationToken cancellationToken) =>
        {
            var items = await service.GetAllAsync(cancellationToken);
            return Results.Ok(items);
        });

        group.MapGet("/{id:int}", async (int id, ITaskService service, CancellationToken cancellationToken) =>
        {
            var task = await service.GetByIdAsync(id, cancellationToken);
            return task is null ? Results.NotFound() : Results.Ok(task);
        });

        group.MapGet("/by-status/{status}", async (TaskStatus status, ITaskService service, CancellationToken cancellationToken) =>
        {
            var items = await service.GetByStatusAsync(status, cancellationToken);
            return Results.Ok(items);
        });

        group.MapPost("/", async (CreateTaskRequest request, ITaskService service, CancellationToken cancellationToken) =>
            {
                var created = await service.CreateAsync(request, cancellationToken);
                return Results.Created($"/api/tasks/{created.Id}", created);
            })
            .RequireAuthorization(policy => policy.RequireRole("Admin", "Manager"));

        group.MapDelete("/{id:int}", async (int id, ITaskService service, CancellationToken cancellationToken) =>
            {
                var deleted = await service.DeleteAsync(id, cancellationToken);
                return deleted ? Results.NoContent() : Results.NotFound();
            })
            .RequireAuthorization(policy => policy.RequireRole("Admin"));

        return app;
    }
}using TaskTracker.Api.Services.Interfaces;

namespace TaskTracker.Api.Endpoints;

public static class UserEndpoints
{
    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/users")
            .WithTags("Users")
            .RequireAuthorization();

        group.MapGet("/me", async (IUserService userService, CancellationToken cancellationToken) =>
        {
            var user = await userService.GetCurrentAsync(cancellationToken);
            return user is null ? Results.NotFound() : Results.Ok(user);
        });

        group.MapGet("/", async (IUserService userService, CancellationToken cancellationToken) =>
            {
                var users = await userService.GetAllAsync(cancellationToken);
                return Results.Ok(users);
            })
            .RequireAuthorization(policy => policy.RequireRole("Admin"));

        return app;
    }
}using TaskTracker.Api.Middleware;

namespace TaskTracker.Api.Extensions;

public static class MiddlewareExtensions
{
    public static IApplicationBuilder UseCustomMiddlewares(this IApplicationBuilder app)
    {
        app.UseMiddleware<ExceptionHandlingMiddleware>();
        app.UseMiddleware<RequestTimingMiddleware>();

        return app;
    }
}using TaskTracker.Api.Auth;
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

        return services;
    }
}using System.Reflection;
using Microsoft.OpenApi;

namespace TaskTracker.Api.Extensions;


public static class SwaggerExtensions
{
    public static IServiceCollection AddCustomSwagger(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "TaskTracker API",
                Version = "v1"
            });
            
            var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);

            options.IncludeXmlComments(xmlPath);
    
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
            {
                Name =  "Authorization",
                Description = "Enter JWT Bearer token",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
            });
    
            options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("Bearer", document)] = []
            });
        });

        return services;
    }
}using AutoMapper;
using TaskTracker.Api.Domain.Entities;
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
    }
}using System.Net;
using System.Text.Json;

namespace TaskTracker.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Business validation error");

            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            context.Response.ContentType = "application/json";

            var payload = new
            {
                error = "bad_request",
                message = ex.Message
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");

            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/json";

            var payload = new
            {
                error = "internal_server_error",
                message = "Unexpected server error."
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
        }
    }
}using System.Diagnostics;

namespace TaskTracker.Api.Middleware;

public class RequestTimingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestTimingMiddleware> _logger;

    public RequestTimingMiddleware(RequestDelegate next, ILogger<RequestTimingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();

        await _next(context);

        stopwatch.Stop();

        _logger.LogInformation(
            "Request {Method} {Path} finished with status {StatusCode} in {ElapsedMs} ms",
            context.Request.Method,
            context.Request.Path,
            context.Response.StatusCode,
            stopwatch.ElapsedMilliseconds);
    }
}namespace TaskTracker.Api.Options;

public class NotificationOptions
{
    public const string SectionName = "Notification";

    public string DefaultSender { get; set; } = null!;
    public bool EnableDueDateReminders { get; set; }
    public int ReminderDaysBefore { get; set; }
}using Microsoft.EntityFrameworkCore;
using TaskTracker.Api.Data;
using TaskTracker.Api.Domain.Entities;
using TaskTracker.Api.Repositories.Interfaces;

namespace TaskTracker.Api.Repositories.Implementations;

public class ProjectRepository : IProjectRepository
{
    private readonly AppDbContext _dbContext;

    public ProjectRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<List<Project>> GetAllAsync(CancellationToken cancellationToken)
    {
        return _dbContext.Projects
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public Task<Project?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        return _dbContext.Projects
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task AddAsync(Project project, CancellationToken cancellationToken)
    {
        await _dbContext.Projects.AddAsync(project, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}using Microsoft.EntityFrameworkCore;
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
}using Microsoft.EntityFrameworkCore;
using TaskTracker.Api.Data;
using TaskTracker.Api.Domain.Entities;
using TaskTracker.Api.Repositories.Interfaces;

namespace TaskTracker.Api.Repositories.Implementations;

public class UserRepository: IUserRepository
{
    private readonly AppDbContext _dbContext;

    public UserRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        return _dbContext.Users.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task<User?> GetByLoginAsync(string login, CancellationToken cancellationToken)
    {
        return _dbContext.Users.FirstOrDefaultAsync(x => x.Login == login, cancellationToken);
    }

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken)
    {
        return _dbContext.Users.FirstOrDefaultAsync(x => x.Email == email, cancellationToken);
    }

    public Task<List<User>> GetAllAsync(CancellationToken cancellationToken)
    {
        return _dbContext.Users
            .AsNoTracking()
            .OrderBy(x => x.Login)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(User user, CancellationToken cancellationToken)
    {
        await _dbContext.Users.AddAsync(user, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}using TaskTracker.Api.Domain.Entities;

namespace TaskTracker.Api.Repositories.Interfaces;

public interface IProjectRepository
{
    Task<List<Project>> GetAllAsync(CancellationToken cancellationToken);
    Task<Project?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task AddAsync(Project project, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}using TaskTracker.Api.Domain.Entities;
using TaskStatus = TaskTracker.Api.Domain.Enums.TaskStatus;

namespace TaskTracker.Api.Repositories.Interfaces;

public interface ITaskRepository
{
    Task<List<TaskItem>> GetAllAsync(CancellationToken cancellationToken);
    Task<TaskItem?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<List<TaskItem>> GetByStatusAsync(TaskStatus status, CancellationToken cancellationToken);
    Task AddAsync(TaskItem task, CancellationToken cancellationToken);
    Task DeleteAsync(TaskItem task, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}using TaskTracker.Api.Domain.Entities;

namespace TaskTracker.Api.Repositories.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<User?> GetByLoginAsync(string login, CancellationToken cancellationToken);
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken);
    Task<List<User>> GetAllAsync(CancellationToken cancellationToken);
    Task AddAsync(User user, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}using TaskTracker.Api.Auth;
using TaskTracker.Api.Domain.Entities;
using TaskTracker.Api.Domain.Enums;
using TaskTracker.Api.Dtos.Auth;
using TaskTracker.Api.Repositories.Interfaces;
using TaskTracker.Api.Services.Interfaces;

namespace TaskTracker.Api.Services.Implementations;

public class AuthService: IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly JwtTokenGenerator _jwtTokenGenerator;
    private readonly PasswordHasher _passwordHasher;

    public AuthService(
        IUserRepository userRepository,
        JwtTokenGenerator jwtTokenGenerator,
        PasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _jwtTokenGenerator = jwtTokenGenerator;
        _passwordHasher = passwordHasher;
    }

    public async Task<UserResponse> RegisterAsync(RegisterUserRequest request, CancellationToken cancellationToken)
    {
        var existingByLogin = await _userRepository.GetByLoginAsync(request.Login, cancellationToken);
        if (existingByLogin is not null)
            throw new InvalidOperationException("Login is already taken.");

        var existingByEmail = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (existingByEmail is not null)
            throw new InvalidOperationException("Email is already taken.");

        var user = new User
        {
            Login = request.Login,
            Email = request.Email,
            FullName = request.FullName,
            Department = request.Department,
            Role = UserRole.User,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };

        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

        await _userRepository.AddAsync(user, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);

        return new UserResponse
        {
            Id = user.Id,
            Login = user.Login,
            Email = user.Email,
            FullName = user.FullName,
            Department = user.Department,
            Role = user.Role
        };
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByLoginAsync(request.Login, cancellationToken);
        if (user is null || !user.IsActive)
            throw new InvalidOperationException("Invalid credentials.");

        var passwordValid = _passwordHasher.VerifyPassword(user, request.Password);
        if (!passwordValid)
            throw new InvalidOperationException("Invalid credentials.");

        var (token, expiresAtUtc) = _jwtTokenGenerator.GenerateToken(
            user.Id,
            user.Login,
            user.Role.ToString(),
            user.Department);

        return new LoginResponse
        {
            AccessToken = token,
            ExpiresAtUtc = expiresAtUtc,
            Login = user.Login,
            Role = user.Role.ToString()
        };
    }
}using Microsoft.Extensions.Options;
using TaskTracker.Api.Options;
using TaskTracker.Api.Services.Interfaces;

namespace TaskTracker.Api.Services.Implementations;

public class DeadlineNotifier : IDeadlineNotifier
{
    private readonly NotificationOptions _options;
    private readonly ILogger<DeadlineNotifier> _logger;

    public DeadlineNotifier(IOptions<NotificationOptions> options, ILogger<DeadlineNotifier> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public Task NotifyAboutDeadlinesAsync(CancellationToken cancellationToken)
    {
        if (!_options.EnableDueDateReminders)
        {
            _logger.LogInformation("Deadline reminders are disabled");
            return Task.CompletedTask;
        }

        _logger.LogInformation(
            "Sending reminders from {Sender}. Days before deadline: {Days}",
            _options.DefaultSender,
            _options.ReminderDaysBefore);

        return Task.CompletedTask;
    }
}using AutoMapper;
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
}using AutoMapper;
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
using TaskTracker.Api.Dtos.Auth;
using TaskTracker.Api.Repositories.Interfaces;
using TaskTracker.Api.Services.Interfaces;

namespace TaskTracker.Api.Services.Implementations;

public class UserService: IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUserAccessor _currentUserAccessor;

    public UserService(IUserRepository userRepository, ICurrentUserAccessor currentUserAccessor)
    {
        _userRepository = userRepository;
        _currentUserAccessor = currentUserAccessor;
    }

    public async Task<List<UserResponse>> GetAllAsync(CancellationToken cancellationToken)
    {
        var users = await _userRepository.GetAllAsync(cancellationToken);

        return users.Select(user => new UserResponse
        {
            Id = user.Id,
            Login = user.Login,
            Email = user.Email,
            FullName = user.FullName,
            Department = user.Department,
            Role = user.Role
        }).ToList();
    }

    public async Task<UserResponse?> GetCurrentAsync(CancellationToken cancellationToken)
    {
        var currentUser = _currentUserAccessor.GetCurrentUser();
        if (!currentUser.IsAuthenticated)
            return null;

        var user = await _userRepository.GetByIdAsync(currentUser.Id, cancellationToken);
        if (user is null)
            return null;

        return new UserResponse
        {
            Id = user.Id,
            Login = user.Login,
            Email = user.Email,
            FullName = user.FullName,
            Department = user.Department,
            Role = user.Role
        };
    }
}using TaskTracker.Api.Dtos.Auth;

namespace TaskTracker.Api.Services.Interfaces;

public interface IAuthService
{
    Task<UserResponse> RegisterAsync(RegisterUserRequest request, CancellationToken cancellationToken);
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken);
}namespace TaskTracker.Api.Services.Interfaces;

public interface IDeadlineNotifier
{
    Task NotifyAboutDeadlinesAsync(CancellationToken cancellationToken);
}using TaskTracker.Api.Dtos.Projects;

namespace TaskTracker.Api.Services.Interfaces;

public interface IProjectService
{
    Task<List<ProjectResponse>> GetAllAsync(CancellationToken cancellationToken);
    Task<ProjectResponse> CreateAsync(CreateProjectRequest request, CancellationToken cancellationToken);
}using TaskTracker.Api.Dtos.Tasks;

namespace TaskTracker.Api.Services.Interfaces;
using TaskStatus = Domain.Enums.TaskStatus;

public interface ITaskService
{
    Task<List<TaskResponse>> GetAllAsync(CancellationToken cancellationToken);
    Task<TaskResponse?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<List<TaskResponse>> GetByStatusAsync(TaskStatus status, CancellationToken cancellationToken);
    Task<TaskResponse> CreateAsync(CreateTaskRequest request, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken);
}using TaskTracker.Api.Dtos.Auth;

namespace TaskTracker.Api.Services.Interfaces;

public interface IUserService
{
    Task<List<UserResponse>> GetAllAsync(CancellationToken cancellationToken);
    Task<UserResponse?> GetCurrentAsync(CancellationToken cancellationToken);
}using FluentValidation;
using TaskTracker.Api.Dtos.Projects;

namespace TaskTracker.Api.Validators;

public class CreateProjectRequestValidator: AbstractValidator<CreateProjectRequest>
{
    public CreateProjectRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Code)
            .NotEmpty()
            .MaximumLength(20)
            .Matches("^[A-Z0-9_-]+$");
    }
}using FluentValidation;
using TaskTracker.Api.Dtos.Tasks;

namespace TaskTracker.Api.Validators;

public class CreateTaskRequestValidator: AbstractValidator<CreateTaskRequest>
{
    public CreateTaskRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(150);

        RuleFor(x => x.Description)
            .MaximumLength(1000);

        RuleFor(x => x.Priority)
            .InclusiveBetween(1, 5);

        RuleFor(x => x.ProjectId)
            .GreaterThan(0);

        RuleFor(x => x.DueDateUtc)
            .Must(x => x == null || x > DateTime.UtcNow)
            .WithMessage("Due date must be in the future.");
    }
}using FluentValidation;
using TaskTracker.Api.Dtos.Tasks;


namespace TaskTracker.Api.Validators;

public class UpdateTaskStatusRequestValidator: AbstractValidator<UpdateTaskStatusRequest>
{
    public UpdateTaskStatusRequestValidator()
    {
        RuleFor(x => x.Status).IsInEnum();
    }
}
это весь код проекта который будет  у меня на кр.Проект намеренно содержит упрощения и неполные участки кода.

