using TeamAvailability.Application.Common.Interfaces;

namespace TeamAvailability.Middleware;

public sealed class ApiKeyAuthenticationMiddleware(RequestDelegate next)
{
    public const string HeaderName = "X-Api-Key";

    public async Task InvokeAsync(HttpContext context, IApiKeyValidator apiKeyValidator)
    {
        if (context.Request.Headers.TryGetValue(HeaderName, out var apiKey) && !string.IsNullOrWhiteSpace(apiKey))
        {
            var result = await apiKeyValidator.ValidateAsync(apiKey!);
            if (result is not null)
            {
                context.Items["ApiKey"] = result;
                context.Items["ApiKeyTeamId"] = result.TeamId;
            }
        }

        await next(context);
    }
}
