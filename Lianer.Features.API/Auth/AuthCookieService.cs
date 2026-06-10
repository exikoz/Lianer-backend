using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;

namespace Lianer.Features.API.App.Auth;

public sealed class AuthCookieService
{
    private readonly AuthCookieSettings _settings;

    public AuthCookieService(IOptions<AuthCookieSettings> options)
    {
        _settings = options.Value;
    }

    public string? ReadAccessTokenCookie(HttpRequest request)
    {
        return request.Cookies.TryGetValue(_settings.AccessCookieName, out var token)
            ? token
            : null;
    }

    public bool HasAuthCookie(HttpRequest request)
    {
        return request.Cookies.ContainsKey(_settings.AccessCookieName)
            || request.Cookies.ContainsKey(_settings.RefreshCookieName);
    }

    public bool HasValidCsrfToken(HttpRequest request)
    {
        var hasCookie = request.Cookies.TryGetValue(
            _settings.CsrfCookieName,
            out var csrfCookie);

        var hasHeader = request.Headers.TryGetValue(
            _settings.CsrfHeaderName,
            out var csrfHeader);

        if (!hasCookie || !hasHeader)
        {
            return false;
        }

        return FixedTimeEquals(csrfCookie!, csrfHeader.ToString());
    }

    private static bool FixedTimeEquals(string left, string right)
    {
        var leftBytes = Encoding.UTF8.GetBytes(left);
        var rightBytes = Encoding.UTF8.GetBytes(right);

        return leftBytes.Length == rightBytes.Length
            && CryptographicOperations.FixedTimeEquals(leftBytes, rightBytes);
    }
}