using Microsoft.AspNetCore.Http;

namespace Lianer.Features.API.App.Auth;

public sealed class AuthCookieSettings
{
    public const string SectionName = "Auth:Cookies";

    public string AccessCookieName { get; init; } = "lianer-access-dev";
    public string RefreshCookieName { get; init; } = "lianer-refresh-dev";
    public string CsrfCookieName { get; init; } = "lianer-csrf-dev";

    public string CsrfHeaderName { get; init; } = "X-CSRF-TOKEN";

    public int AccessTokenMinutes { get; init; } = 15;
    public int RefreshTokenDays { get; init; } = 7;

    public bool Secure { get; init; } = false;
    public SameSiteMode SameSite { get; init; } = SameSiteMode.Lax;

    public string Path { get; init; } = "/";
    public bool IsEssential { get; init; } = true;
}