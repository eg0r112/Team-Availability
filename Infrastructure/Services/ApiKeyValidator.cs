using Microsoft.EntityFrameworkCore;
using TeamAvailability.Application.Common.Interfaces;
using TeamAvailability.Infrastructure.Persistence;

namespace TeamAvailability.Infrastructure.Services;

public sealed class ApiKeyValidator(ApplicationDbContext db, IPasswordHasher passwordHasher) : IApiKeyValidator
{
    public async Task<ApiKeyValidationResult?> ValidateAsync(string apiKey, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(apiKey) || apiKey.Length < 8)
            return null;

        var prefix = apiKey[..8];
        var candidates = await db.ApiKeys
            .AsNoTracking()
            .Where(k => k.IsActive && k.KeyPrefix == prefix)
            .ToListAsync(cancellationToken);

        foreach (var candidate in candidates)
        {
            if (candidate.ExpiresAt.HasValue && candidate.ExpiresAt <= DateTimeOffset.UtcNow)
                continue;

            if (passwordHasher.Verify(apiKey, candidate.KeyHash))
                return new ApiKeyValidationResult(candidate.Id, candidate.TeamId, candidate.Name);
        }

        return null;
    }
}
