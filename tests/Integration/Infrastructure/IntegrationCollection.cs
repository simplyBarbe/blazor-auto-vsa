using Xunit;

namespace Integration.Infrastructure;

/// <summary>
/// Shared collection so all integration tests use a single TestWebApplicationFactory instance.
/// Prevents parallel server startup and "no web application was configured" errors.
/// </summary>
[CollectionDefinition(Name)]
public sealed class IntegrationCollection : ICollectionFixture<TestWebApplicationFactory>
{
    public const string Name = nameof(IntegrationCollection);
}
