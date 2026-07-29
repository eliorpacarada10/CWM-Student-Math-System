using CWM.Adapters.Persistence;
using CWM.Application.Ports.Driven;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;

namespace CWM.Tests.ComponentTests;

/// <summary>
/// Boots the real Api host -- real controllers, real Application use cases, real MathEngine
/// and XmlParsing adapters -- but replaces IExamRepository (the one adapter with actual I/O)
/// with a Moq mock. This is "through the controller and everything in between, with mocking":
/// it proves the HTTP boundary, routing, validation, and orchestration all wire together
/// correctly, without needing a real database.
/// </summary>
public sealed class ComponentWebApplicationFactory : WebApplicationFactory<Program>
{
    public const string TestApiKey = "component-test-api-key";

    public Mock<IExamRepository> RepositoryMock { get; } = new();

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
            // Component tests never touch the database at all -- IExamRepository is
            // mocked below -- so CwmDbContext itself must also be removed, not just its
            // options. Leaving CwmDbContext registered with no DbContextOptions to resolve
            // fails ASP.NET Core's build-time service validation before any test can run.
            services.RemoveAll<DbContextOptions<CwmDbContext>>();
            services.RemoveAll<CwmDbContext>();
            services.RemoveAll<IExamRepository>();
            services.AddScoped(_ => RepositoryMock.Object);
        });
    }

    public HttpClient CreateAuthenticatedClient()
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Add("X-Api-Key", TestApiKey);
        return client;
    }
}
