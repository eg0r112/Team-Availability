using TeamAvailability.Application.Common.Interfaces;

namespace TeamAvailability.Infrastructure.Services;

public sealed class DateTimeProvider : IDateTimeProvider
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
