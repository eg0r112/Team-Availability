namespace TeamAvailability.Infrastructure.Options;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; init; } = "TeamAvailability";
    public string Audience { get; init; } = "TeamAvailabilityClients";
    public string Secret { get; init; } = string.Empty;
    public int AccessTokenMinutes { get; init; } = 60;
}
