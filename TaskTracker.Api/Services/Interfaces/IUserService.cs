using TaskTracker.Api.Dtos.Auth;

namespace TaskTracker.Api.Services.Interfaces;

public interface IUserService
{
    Task<List<UserResponse>> GetAllAsync(CancellationToken cancellationToken);
    Task<UserResponse?> GetCurrentAsync(CancellationToken cancellationToken);
}