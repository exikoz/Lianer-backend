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
            builder.Services.AddMemoryCache();
            builder.Services
            .SetupInMemoryDb() 
            .SetupServices() 
            .SetupRepositories() 
            .SetupGoogleAuth() 
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
        app.MapControllers().RequireRateLimiting("fixed");
        app.Run();
        }
    }
}
