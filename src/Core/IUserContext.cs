using System.Security.Claims;

namespace MedConnect.Core;

public interface IUserContext
{
    UserIdentity Identity { get; }
}

public class UserContext(IHttpContextAccessor httpContextAccessor) : IUserContext
{
    public UserIdentity Identity => new(httpContextAccessor.HttpContext?.User);
}

public sealed class UserIdentity
{
    private readonly ClaimsPrincipal? _user;

    public UserIdentity(ClaimsPrincipal? user)
    {
        _user = user;
    }

    public Guid Id
    {
        get
        {
            var claim = _user?.FindFirst(ClaimTypes.NameIdentifier) ?? _user?.FindFirst("sub");

            if (claim is null || !Guid.TryParse(claim.Value, out var userId))
            {
                throw new UnauthorizedAccessException("Brak zalogowanego użytkownika lub niepoprawne ID w tokenie.");
            }

            return userId;
        }
    }

    public string? Role => _user?.FindFirst(ClaimTypes.Role)?.Value ?? _user?.FindFirst("role")?.Value ?? "User";

    public string? Email => _user?.FindFirst(ClaimTypes.Email)?.Value ?? _user?.FindFirst("email")?.Value;
}