using TaskTracker.Api.Services.Interfaces;

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
}