namespace TeamAvailability.Infrastructure.Options;

public sealed class LocalDevOptions
{
    public const string SectionName = "LocalDev";

    /// <summary>Без Docker: in-memory cache вместо Redis.</summary>
    public bool UseMemoryCache { get; init; } = true;

    /// <summary>Без Docker: in-memory bus вместо RabbitMQ.</summary>
    public bool UseInMemoryBus { get; init; } = true;
}
