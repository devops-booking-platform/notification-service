using System.Security.Claims;
using NotificationService.Services.Interfaces;

namespace NotificationService.Services;

public class CurrentUserService(IHttpContextAccessor accessor) : ICurrentUserService
{
    public Guid? UserId => Guid.Parse(accessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value!);

    public string? Role => accessor.HttpContext?.User?.FindFirst(ClaimTypes.Role)?.Value;

    public bool IsAuthenticated =>
        accessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
}