namespace Lianer.Core.API.App.Auth;

public sealed class AuthCookieSettings
{
    public const string SectionName = "Auth:Cookies";

    public string AccessCookieName { get; init; } = "__Host-lianer-access";
    public string RefreshCookieName { get; init; } = "__Host-lianer-refresh";
    public string CsrfCookieName { get; init; } = "__Host-lianer-csrf";

    public string CsrfHeaderName { get; init; } = "X-CSRF-TOKEN";

    public int AccessTokenMinutes { get; init; } = 15;
    public int RefreshTokenDays { get; init; } = 7;

    public bool Secure { get; init; } = true;
    public SameSiteMode SameSite { get; init; } = SameSiteMode.Strict;

    public string Path { get; init; } = "/";
    public bool IsEssential { get; init; } = true;
}