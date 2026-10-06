namespace TeamAvailability.Contracts.Auth;

public sealed record RegisterRequest(string Email, string Password, string DisplayName);
