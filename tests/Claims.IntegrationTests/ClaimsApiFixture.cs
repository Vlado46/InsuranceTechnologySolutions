using Claims.IntegrationTests;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Testcontainers.MongoDb;
using Testcontainers.MsSql;
using Xunit;

[assembly: AssemblyFixture(typeof(ClaimsApiFixture))]

namespace Claims.IntegrationTests;

/// <summary>Runs the real API against real databases. All tests share one set of containers.</summary>
public sealed class ClaimsApiFixture : IAsyncLifetime
{
    private readonly MsSqlContainer _sqlContainer = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest").Build();
    private readonly MongoDbContainer _mongoContainer = new MongoDbBuilder("mongo:8").Build();

    private WebApplicationFactory<Program> _factory = null!;

    public async ValueTask InitializeAsync()
    {
        await Task.WhenAll(_sqlContainer.StartAsync(), _mongoContainer.StartAsync());

        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Audit", _sqlContainer.GetConnectionString());
            builder.UseSetting("ConnectionStrings:Mongo", _mongoContainer.GetConnectionString());
        });
    }

    public async ValueTask DisposeAsync()
    {
        await _factory.DisposeAsync();
        await _sqlContainer.DisposeAsync();
        await _mongoContainer.DisposeAsync();
    }

    public HttpClient CreateClient() => _factory.CreateClient();
}
