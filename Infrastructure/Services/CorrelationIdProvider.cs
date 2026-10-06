using Microsoft.AspNetCore.Http;
using TeamAvailability.Application.Common.Interfaces;

namespace TeamAvailability.Infrastructure.Services;

public sealed class CorrelationIdProvider(IHttpContextAccessor httpContextAccessor) : ICorrelationIdProvider
{
    public string CorrelationId =>
        httpContextAccessor.HttpContext?.Items["CorrelationId"]?.ToString()
        ?? httpContextAccessor.HttpContext?.Request.Headers["X-Correlation-Id"].FirstOrDefault()
        ?? Guid.NewGuid().ToString("N");
}
