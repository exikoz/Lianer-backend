using System.Text;
using System.Threading.RateLimiting;
using Asp.Versioning;
using Azure.Identity;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Lianer.Core.API.Data;
using Lianer.Core.API.Filters;
using Lianer.Core.API.Middleware;
using Lianer.Core.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;
using Lianer.Core.API.Config;

namespace Lianer.Core.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            // --- Azure Key Vault (Always active) ---
            if (builder.Environment.IsProduction())
            {
                builder.SetupAzureKeyVault();
            }
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
