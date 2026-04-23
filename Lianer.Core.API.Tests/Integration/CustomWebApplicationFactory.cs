using Lianer.Core.API.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Lianer.Core.API.Tests.Integration;

/// <summary>
/// Anpassad WebApplicationFactory som konfigurerar testmiljön.
/// Ersätter databasen med en unik InMemory-databas per test
/// och sätter JWT-konfiguration så att appen startar utan User Secrets.
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "JwtSettings:SecretKey", "IntegrationTestSecretKeyMinstTrettioTvåTecken!!" },
                { "JwtSettings:Issuer", "TestIssuer" },
                { "JwtSettings:Audience", "TestAudience" },
                { "JwtSettings:ExpirationMinutes", "30" }
            });
        });

        builder.ConfigureServices(services =>
        {
            // Ta bort befintlig DbContext-registrering
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor != null)
                services.Remove(descriptor);

            // Använd ett fast namn för databasen så att den delas mellan anrop i samma testkörning
            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase("IntegrationTestDb"));
        });
    }
}
