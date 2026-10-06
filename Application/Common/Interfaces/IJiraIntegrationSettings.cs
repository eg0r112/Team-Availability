namespace TeamAvailability.Application.Common.Interfaces;

public interface IJiraIntegrationSettings
{
    bool Enabled { get; }
    string ProjectKey { get; }
    string IssueTypeId { get; }
    string BaseUrl { get; }
}
