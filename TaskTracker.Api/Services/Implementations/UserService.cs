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
}