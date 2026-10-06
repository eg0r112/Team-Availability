namespace TeamAvailability.Contracts.Auth;

public sealed record TokenResponse(string AccessToken, string RefreshToken, DateTimeOffset ExpiresAt);
