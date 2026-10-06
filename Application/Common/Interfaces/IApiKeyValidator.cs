namespace TeamAvailability.Application.Common.Interfaces;

public interface IApiKeyValidator
{
    Task<ApiKeyValidationResult?> ValidateAsync(string apiKey, CancellationToken cancellationToken = default);
}

public sealed record ApiKeyValidationResult(Guid ApiKeyId, Guid? TeamId, string Name);
