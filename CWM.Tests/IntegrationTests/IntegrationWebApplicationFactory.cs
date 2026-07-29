using CWM.Adapters.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CWM.Tests.IntegrationTests;

/// <summary>
/// Boots the real Api host with every adapter real, including persistence -- except SQL
/// Server is swapped for SQLite's in-memory mode. SQLite is a genuine relational engine (real
/// SQL, real unique constraints, real cascade deletes), unlike EF Core's InMemory provider,
/// which Microsoft's own docs advise against using to validate relational behavior. This is
/// "as real as it gets" without standing up an actual SQL Server instance for CI.
///
/// One instance = one isolated in-memory database. Tests must not share an instance via
/// IClassFixture, or they will see each other's persisted data -- create a fresh instance
/// per test instead.
/// </summary>
public sealed class IntegrationWebApplicationFactory : WebApplicationFactory<Program>
{
    public const string TestApiKey = "integration-test-api-key";

    private readonly SqliteConnection _connection;

    public IntegrationWebApplicationFactory()
    {
        // An in-memory SQLite database is destroyed when its last connection closes, so this
        // connection must stay open for the lifetime of the factory.
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        using var scope = Services.CreateScope();
        scope.ServiceProvider.GetRequiredService<CwmDbContext>().Database.EnsureCreated();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Auth:ApiKey"] = TestApiKey
            });
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<CwmDbContext>>();
            services.AddDbContext<CwmDbContext>(options => options.UseSqlite(_connection));
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            _connection.Dispose();
        }
    }

    public HttpClient CreateAuthenticatedClient()
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Add("X-Api-Key", TestApiKey);
        return client;
    }
}
