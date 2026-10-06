namespace TeamAvailability.Application.Common.Interfaces;

public interface ICorrelationIdProvider
{
    string CorrelationId { get; }
}
