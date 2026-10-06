using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using TeamAvailability.Application.Common.Interfaces;
using TeamAvailability.Domain.Enums;

namespace TeamAvailability.Infrastructure.Services;

public sealed class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    private ClaimsPrincipal? User => httpContextAccessor.HttpContext?.User;

    public Guid? UserId => Guid.TryParse(User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? User?.FindFirstValue("sub"), out var id) ? id : null;

    public string? Email => User?.FindFirstValue(ClaimTypes.Email);

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

    public bool IsAdmin => User?.IsInRole(SystemRole.Admin.ToString()) ?? false;

    public IReadOnlyCollection<Guid> TeamIds =>
        User?.FindAll("team_id").Select(c => Guid.Parse(c.Value)).ToArray() ?? [];
}
