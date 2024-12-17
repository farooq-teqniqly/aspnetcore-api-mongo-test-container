using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Testcontainers.MongoDb;

namespace ApiTests;

public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
{
    private static readonly MongoDbContainer _mongoDbContainer;

    static CustomWebApplicationFactory()
    {
        _mongoDbContainer = new MongoDbBuilder()
            .WithImage("mongo:7.0.15")
            .WithCleanUp(true)
            .Build();

        _mongoDbContainer.StartAsync().GetAwaiter().GetResult();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("development");

        var connectionString = _mongoDbContainer.GetConnectionString();
        var mongoClient = new MongoClient(connectionString);
        var mongoDatabase = mongoClient.GetDatabase("envino-test");

        builder.ConfigureServices(services =>
        {
            var descriptor = services.Single(s => s.ServiceType == typeof(IMongoDatabase));
            services.Remove(descriptor);

            services.AddScoped<IMongoDatabase>(_ => mongoDatabase);
        });
    }
}
