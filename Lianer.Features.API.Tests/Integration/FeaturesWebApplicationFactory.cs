using Lianer.Features.API.Clients;
using Lianer.Features.API.Data;
using Lianer.Features.API.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Lianer.Features.API.Tests.Integration;

/// <summary>
/// Custom WebApplicationFactory for Features API integration tests.
/// Replaces the database with a unique InMemory instance,
/// mocks external services (Hunter.io, Core API) and configures JWT for testing.
/// </summary>
public class FeaturesWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = $"FeaturesTestDb_{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "JwtSettings:SecretKey", "FeaturesTestSecretKeyMinstTrettioTvåTecken!!" },
                { "JwtSettings:Issuer", "TestIssuer" },
                { "JwtSettings:Audience", "TestAudience" },
                { "JwtSettings:ExpirationMinutes", "30" },
                { "AzureKeyVault:VaultUri", "" },
                { "Hunter:ApiKey", "test-hunter-key" }
            });
        });

        builder.ConfigureServices(services =>
        {
            // Remove existing DbContext registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<FeaturesDbContext>));
            if (descriptor != null)
                services.Remove(descriptor);

            // Use a unique InMemory database per factory instance
            services.AddDbContext<FeaturesDbContext>(options =>
                options.UseInMemoryDatabase(_dbName));

            // Replace HunterService with a mock that doesn't call external APIs
            var hunterDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(IHunterService));
            if (hunterDescriptor != null)
                services.Remove(hunterDescriptor);

            services.AddScoped<IHunterService, FakeHunterService>();

            // Remove typed client registrations that would fail without real endpoints
            var hunterClientDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(HunterClient));
            if (hunterClientDescriptor != null)
                services.Remove(hunterClientDescriptor);

            var coreClientDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(CoreApiClient));
            if (coreClientDescriptor != null)
                services.Remove(coreClientDescriptor);

            // Register a CoreApiClient with a fake HttpClient
            services.AddScoped(sp =>
            {
                var httpClient = new HttpClient(new FakeCoreApiHandler())
                {
                    BaseAddress = new Uri("http://localhost:5297/")
                };
                var logger = sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<CoreApiClient>>();
                return new CoreApiClient(httpClient, logger);
            });
        });
    }
}
