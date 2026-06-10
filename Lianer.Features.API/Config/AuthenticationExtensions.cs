using System.Text;
using Lianer.Features.API.App.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Lianer.Features.API.Config;

public static class AuthenticationExtensions
{
    public static IServiceCollection SetupAuthCookies(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        services
            .AddOptions<AuthCookieSettings>()
            .Bind(configuration.GetSection(AuthCookieSettings.SectionName))
            .Validate(settings => !string.IsNullOrWhiteSpace(settings.AccessCookieName),
                "AccessCookieName is required.")
            .Validate(settings => !string.IsNullOrWhiteSpace(settings.CsrfCookieName),
                "CsrfCookieName is required.")
            .Validate(settings => !string.IsNullOrWhiteSpace(settings.CsrfHeaderName),
                "CsrfHeaderName is required.")
            .Validate(settings => settings.Path == "/",
                "Auth cookies should use Path='/' for app-wide authentication.")
            .Validate(settings => settings.Secure || environment.IsDevelopment(),
                "Auth cookies must be Secure outside development.")
            .ValidateOnStart();

        services.AddScoped<AuthCookieService>();

        return services;
    }

    public static IServiceCollection SetupJwt(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        var jwtSettings = configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"];

        if (string.IsNullOrWhiteSpace(secretKey))
        {
            if (environment.IsProduction())
            {
                throw new InvalidOperationException("JWT SecretKey MUST be configured in Production!");
            }

            secretKey = "LianerBackendSharedDevelopmentSecretKey2026!!";
        }

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,

                    //  must match Core API token !
                    ValidIssuer = jwtSettings["Issuer"] ?? "http://localhost:5297",
                    ValidAudience = jwtSettings["Audience"] ?? "http://localhost:5297",

                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(secretKey)),

                    ClockSkew = TimeSpan.Zero
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = ctx =>
                    {
                        var hasAuthorizationHeader =
                            ctx.Request.Headers.ContainsKey("Authorization");

                        if (hasAuthorizationHeader)
                        {
                            return Task.CompletedTask;
                        }

                        var cookieService = ctx.HttpContext
                            .RequestServices
                            .GetRequiredService<AuthCookieService>();

                        var accessToken = cookieService.ReadAccessTokenCookie(
                            ctx.HttpContext.Request);

                        if (!string.IsNullOrWhiteSpace(accessToken))
                        {
                            ctx.Token = accessToken;
                        }

                        return Task.CompletedTask;
                    }
                };
            });

        services.AddAuthorization();

        return services;
    }
}