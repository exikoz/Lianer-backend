using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

public static class AuthenticationExtensions
{
    public static IServiceCollection SetupJwt(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        // --- Configure JWT Authentication ---
        var jwtSettings = configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"];

    
        if (string.IsNullOrEmpty(secretKey))
        {
            if (environment.IsProduction())
            {
                throw new InvalidOperationException("JWT SecretKey MUST be configured in Production!");
            }
            // Fallback för Development/Testing
            secretKey = "LianerBackendSharedDevelopmentSecretKey2026!!"; 
        }

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options => {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings["Issuer"] ?? "http://localhost:5297",
                ValidAudience = jwtSettings["Audience"] ?? "http://localhost:5297",
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                ClockSkew = TimeSpan.Zero
                };
            });

            services.AddAuthorization(); // kan bli egen funktion vid uppskalning
            return services;
        }
}