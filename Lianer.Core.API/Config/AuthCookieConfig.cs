
using Lianer.Core.API.App.Auth;
using Microsoft.AspNetCore.Http;

namespace Lianer.Core.API.Config;

public static class AuthCookieConfig
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
            .Validate(settings => !string.IsNullOrWhiteSpace(settings.RefreshCookieName),
                "RefreshCookieName is required.")
            .Validate(settings => !string.IsNullOrWhiteSpace(settings.CsrfCookieName),
                "CsrfCookieName is required.")
            .Validate(settings => !string.IsNullOrWhiteSpace(settings.CsrfHeaderName),
                "CsrfHeaderName is required.")
            .Validate(settings => settings.AccessTokenMinutes > 0,
                "AccessTokenMinutes must be greater than 0.")
            .Validate(settings => settings.RefreshTokenDays > 0,
                "RefreshTokenDays must be greater than 0.")
            .Validate(settings => settings.Path == "/",
                "Auth cookies should use Path='/' for app-wide authentication.")
            .Validate(settings => settings.Secure || environment.IsDevelopment(),
                "Auth cookies must be Secure outside development.")
            .Validate(settings => settings.SameSite != SameSiteMode.None || settings.Secure,
                "SameSite=None requires Secure=true.")
            .Validate(settings =>
                    !settings.AccessCookieName.StartsWith("__Host-", StringComparison.Ordinal)
                    || settings.Secure,
                "__Host- access cookies must be Secure.")
            .Validate(settings =>
                    !settings.RefreshCookieName.StartsWith("__Host-", StringComparison.Ordinal)
                    || settings.Secure,
                "__Host- refresh cookies must be Secure.")
            .Validate(settings =>
                    !settings.CsrfCookieName.StartsWith("__Host-", StringComparison.Ordinal)
                    || settings.Secure,
                "__Host- CSRF cookies must be Secure.")
            .ValidateOnStart();

        services.AddScoped<AuthCookieService>();

        return services;
    }
}