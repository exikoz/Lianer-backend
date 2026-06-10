using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;


namespace Lianer.Core.API.App.Auth;

public interface IAuthCookieService
{
    void CreateAccessCookie(HttpResponse response, string accessJwt);
    void CreateCsrfCookie(HttpResponse response);
    void CreateRefreshCookie(HttpResponse response, string refreshToken);
    void DeleteAuthenticationCookies(HttpResponse response);
    bool HasAuthCookie(HttpRequest request);
    bool HasValidCsrfToken(HttpRequest request);
    string? ReadAccessTokenCookie(HttpRequest request);
    string? ReadRefreshTokenCookie(HttpRequest request);
    void SetAuthenticationCookies(HttpResponse response, string accessJwt, string refreshToken);
}

public sealed class AuthCookieService : IAuthCookieService
{
    private readonly AuthCookieSettings _settings;
    public AuthCookieService(IOptions<AuthCookieSettings> options)
    {
        _settings = options.Value;
    }
    public void SetAuthenticationCookies(
        HttpResponse response,
        string accessJwt,
        string refreshToken)
    {
        CreateAccessCookie(response, accessJwt);
        CreateRefreshCookie(response, refreshToken);
        CreateCsrfCookie(response);
    }

    /*
            We use [Authorize] attributes on protected endpoints.
            JwtBearer authentication normally expects the token in the
            Authorization: Bearer <token> header.

            Since the frontend now uses HttpOnly cookies, JwtBearer is configured
            to extract the JWT from the access-token cookie instead.
    */
    public void CreateAccessCookie(HttpResponse response, string accessJwt)
    {
        response.Cookies.Append
        (
            _settings.AccessCookieName,
            accessJwt,
            GetAccessCookieOptions()
        );
    }

    private CookieOptions GetAccessCookieOptions()
    {
        return new CookieOptions
        {
            // This flag ensures no JS running in the web browser can read the cookie.
            HttpOnly = true,

            Secure = _settings.Secure,
            SameSite = _settings.SameSite,
            Path = _settings.Path,
            Expires = DateTimeOffset.UtcNow.AddMinutes(_settings.AccessTokenMinutes),
            IsEssential = _settings.IsEssential
        };
    }

    /*
        Used to refresh our AccessTokens so the
        user doesn't have to login/out to generate one.
        Only works while the user has a valid refresh token.
    */
    public void CreateRefreshCookie(HttpResponse response, string refreshToken)
    {
        response.Cookies.Append
        (
            _settings.RefreshCookieName,
            refreshToken,
            GetRefreshCookieOptions()
        );
    }

    private CookieOptions GetRefreshCookieOptions()
    {
        return new CookieOptions
        {
            HttpOnly = true,
            Secure = _settings.Secure,
            SameSite = _settings.SameSite,
            Path = _settings.Path,


            Expires = DateTimeOffset.UtcNow.AddDays(_settings.RefreshTokenDays),

            IsEssential = _settings.IsEssential
        };
    }

    /*
        This cookie gives the api an  extra layer of protection. 
        It protects against Cross-site Request Forgery attacks. 
        So if some site tries to trick the browser into sending
        a request with our auth cookies, the api will still reject it
        unless the request also sends the correct CSRF token in the header.


    */
    public void CreateCsrfCookie(HttpResponse response)
    {
        /* to make it hard to guess we encode 256-bits of data into a string
        */
        var csrfToken = GenerateSecureToken();

        response.Cookies.Append
        (
            _settings.CsrfCookieName,
            csrfToken,
            GetCsrfCookieOptions()
        );
    }

    
    private CookieOptions GetCsrfCookieOptions()
    {
        return new CookieOptions
        {
            HttpOnly = false,
            Secure = _settings.Secure,
            SameSite = _settings.SameSite,
            Path = _settings.Path,
            Expires = DateTimeOffset.UtcNow.AddDays(_settings.RefreshTokenDays),
            IsEssential = _settings.IsEssential
        };
    }
    /*
        Here we extract the content of the cookie, in this case the jwt token. 
    */
    public string? ReadAccessTokenCookie(HttpRequest request)
    {
        return request.Cookies.TryGetValue(_settings.AccessCookieName, out var token)
            ? token
            : null;
    }

    public string? ReadRefreshTokenCookie(HttpRequest request)
    {
        return request.Cookies.TryGetValue(_settings.RefreshCookieName, out var token)
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

    /*
        Deletes all authentication related cookies
    */
    public void DeleteAuthenticationCookies(HttpResponse response)
    {
        response.Cookies.Delete
        (
            _settings.AccessCookieName,
            CreateDeleteCookieOptions()
        );

        response.Cookies.Delete
        (
            _settings.RefreshCookieName,
            CreateDeleteCookieOptions()
        );

        response.Cookies.Delete
        (
            _settings.CsrfCookieName,
            CreateDeleteCookieOptions()
        );
    }

    private CookieOptions CreateDeleteCookieOptions()
    {
        return new CookieOptions
        {
            Secure = _settings.Secure,
            SameSite = _settings.SameSite,
            Path = _settings.Path
        };
    }

    /*
                HELPERS
    */

    private static string GenerateSecureToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes);
    }

    /*
        User for our CSRF cookie validation
        Ensured the random bits frontend sends back are still
        the same as the one backend kept.
    */
    private static bool FixedTimeEquals(string left, string right)
    {
        var leftBytes = Encoding.UTF8.GetBytes(left);
        var rightBytes = Encoding.UTF8.GetBytes(right);

        /*
           String comparison methods stops executing when
           a difference is found for logical reasons. 
           This also means that someone could theoretically 
           figure out how much of a token was correct by measuring
           the time differences. FixedTimeEquals is a better
           use than normal string comparisons for this usecase. As it 
           keeps comparing indescriminatly. 
        */
        return leftBytes.Length == rightBytes.Length
            && CryptographicOperations.FixedTimeEquals(leftBytes, rightBytes);
    }

// this is just for dev use 
public object GetUserInfoFromCookie(HttpContext httpContext)
{
    var request = httpContext.Request;
    var user = httpContext.User;

    var userId =
        user.FindFirstValue("userId")
        ?? user.FindFirstValue(ClaimTypes.NameIdentifier);

    var email =
        user.FindFirstValue("email")
        ?? user.FindFirstValue(ClaimTypes.Email);

    var fullName =
        user.FindFirstValue("fullName")
        ?? user.FindFirstValue(ClaimTypes.Name);

    return new
    {
        hasAccessCookie = request.Cookies.ContainsKey(_settings.AccessCookieName),
        hasRefreshCookie = request.Cookies.ContainsKey(_settings.RefreshCookieName),
        hasCsrfCookie = request.Cookies.ContainsKey(_settings.CsrfCookieName),

        authenticated = user.Identity?.IsAuthenticated ?? false,
        authenticationType = user.Identity?.AuthenticationType,

        user = new
        {
            userId,
            email,
            fullName
        },

        cookieNames = new
        {
            access = _settings.AccessCookieName,
            refresh = _settings.RefreshCookieName,
            csrf = _settings.CsrfCookieName
        }
    };
}
}