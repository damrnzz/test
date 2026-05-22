using TaskTracker.Api.Auth;
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
}