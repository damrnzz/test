using TaskTracker.Api.Dtos.Projects;
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
        group.MapGet("/",)

        return app;
    }
}