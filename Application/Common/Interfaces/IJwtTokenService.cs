using TeamAvailability.Domain.Entities;

namespace TeamAvailability.Application.Common.Interfaces;

public interface IJwtTokenService
{
    (string AccessToken, DateTimeOffset ExpiresAt) GenerateAccessToken(User user);
    string GenerateRefreshToken();
}
