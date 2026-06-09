using Lianer.Core.API.Config;

namespace Lianer.Core.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            // --- Azure Key Vault (Always active) ---
            builder.SetupAzureKeyVault();
            builder.Services.SetupTelemetry(builder.Configuration);
            builder.Services.AddMemoryCache();
            builder.Services
            .SetupInMemoryDb(builder.Environment) 
            .SetupServices() 
            .SetupRepositories() 
            .SetupGoogleAuth()
            .SetupGeminiClient()
            .SetupJwt(builder.Configuration, builder.Environment)
            .SetupCorsPolicy(builder.Configuration)
            .SetupRateLimiting()
            .SetupOpenAPI()            
            .SetupApiVersioning()
            .SetupControllers();
        builder.Host.SetupValidation();
        var app = builder.Build();
        app.InitDatabase();
        app.SetupMiddleware();
        app.SetupDevelopment(builder.Configuration);
        app.MapGet("/", () => app.Environment.IsDevelopment() 
            ? Results.Redirect("/scalar/v1") 
            : Results.Ok(new { status = "Online", service = "Lianer Core API" }));
        app.MapGet("/favicon.ico", () => Results.Ok());

        app.MapControllers().RequireRateLimiting("fixed");
        app.Run();
        }
    }
}
