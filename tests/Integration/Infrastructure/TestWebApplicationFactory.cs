using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace Integration.Infrastructure;

/// <summary>
/// Web application factory that uses an in-memory database for integration tests.
/// </summary>
public class TestWebApplicationFactory : WebApplicationFactory<Server.Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["UseInMemoryDatabase"] = "true"
            });
        });
    }
}
