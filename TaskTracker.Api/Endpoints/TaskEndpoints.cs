using TaskTracker.Api.Dtos.Tasks;
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
}