using TaskTracker.Api.Auth;
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
}