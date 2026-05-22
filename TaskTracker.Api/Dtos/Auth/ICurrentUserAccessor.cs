namespace TaskTracker.Api.Dtos.Auth;

public interface ICurrentUserAccessor
{
    CurrentUser GetCurrentUser();
}