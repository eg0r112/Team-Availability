using Xunit;

namespace TeamAvailability.Tests.Infrastructure;

[CollectionDefinition("JiraIntegration")]
public sealed class JiraIntegrationCollection : ICollectionFixture<JiraEnabledAppFactory>;
