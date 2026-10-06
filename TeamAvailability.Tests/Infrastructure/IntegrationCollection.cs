using Xunit;

namespace TeamAvailability.Tests.Infrastructure;

[CollectionDefinition("Integration")]
public sealed class IntegrationCollection : ICollectionFixture<TeamAvailabilityAppFactory>;
