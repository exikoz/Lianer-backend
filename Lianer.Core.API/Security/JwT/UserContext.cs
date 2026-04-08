
using System.Security.Claims;
public sealed class UserContext(IHttpContextAccessor http)
{

    // instance of the http request context
    private readonly IHttpContextAccessor _http = http;
   
    public Guid? UserId
    {
        get
            {
                var value = _http.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
                // This methods only responsibility is to try and parse claims. 
                // So I only send back null if data is malformed/missing/corrupt. 
                return Guid.TryParse(value, out var userId)
                ? userId : null;
            }
    }

    // Returns username 
    public string? Username => _http.HttpContext?.User.Identity?.Name;
    // authentication check - simple bool
    public bool? IsAuth => _http.HttpContext?.User.Identity?.IsAuthenticated;
}