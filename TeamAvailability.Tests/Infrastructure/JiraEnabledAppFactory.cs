namespace TeamAvailability.Tests.Infrastructure;

public sealed class JiraEnabledAppFactory : TeamAvailabilityAppFactory
{
    public JiraEnabledAppFactory() => EnableJiraIntegration = true;
}
