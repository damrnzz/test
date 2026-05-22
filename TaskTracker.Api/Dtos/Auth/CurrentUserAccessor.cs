using System.Security.Claims;

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
}