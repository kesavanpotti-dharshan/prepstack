using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Testcontainers.MongoDb;

namespace Prepstack.Api.IntegrationTests;

public sealed class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly MongoDbContainer _mongoContainer = new MongoDbBuilder("mongo:7.0").Build();

    public Task InitializeAsync() => _mongoContainer.StartAsync();

    async Task IAsyncLifetime.DisposeAsync()
    {
        await _mongoContainer.DisposeAsync();
        await base.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("Mongo:ConnectionString", _mongoContainer.GetConnectionString());
        builder.UseSetting("Mongo:DatabaseName", "prepstack-tests");
    }
}
