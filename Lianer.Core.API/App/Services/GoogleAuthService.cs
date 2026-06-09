using System.Text.Json;
using Lianer.Core.API.DTOs.Auth;
using Polly;

namespace Lianer.Core.API.Services;

/// <summary>
/// Service for Google OAuth2 authentication with resilience patterns
/// </summary>
public class GoogleAuthService : IGoogleAuthService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<GoogleAuthService> _logger;
    private readonly IConfiguration _configuration;

    public GoogleAuthService(IHttpClientFactory httpClientFactory, ILogger<GoogleAuthService> logger, IConfiguration configuration)
    {
        _httpClient = httpClientFactory.CreateClient("GoogleAuth");
        _logger = logger;
        _configuration = configuration;
    }

    /// <summary>
    /// Generates the Google OAuth2 authorization URL for the client to redirect to
    /// Uses implicit flow (token in URL fragment) for client-side authentication
    /// </summary>
    /// <returns>The complete Google authorization URL</returns>
    public string GetGoogleLoginUrl()
    {
        var clientId = _configuration["GoogleAuth:ClientId"] ?? _configuration["Google:Auth:ClientId"];
        var redirectUri = _configuration["GoogleAuth:RedirectUri"] ?? _configuration["Google:Auth:RedirectUri"];
        var baseUrl = "https://accounts.google.com/o/oauth2/v2/auth";

        // Use token response_type for implicit flow (client-side)
        var url = $"{baseUrl}?" +
                  $"client_id={clientId}&" +
                  $"redirect_uri={Uri.EscapeDataString(redirectUri ?? string.Empty)}&" +
                  "response_type=token&" +  // Changed from 'code' to 'token' for implicit flow
                  "scope=openid%20email%20profile&" +
                  "include_granted_scopes=true";

        _logger.LogInformation("Generated Google login URL with redirect_uri: {RedirectUri}", redirectUri);
        
        return url;
    }

    /// <summary>
    /// Validates Google access token and retrieves user information with resilience patterns
    /// </summary>
    public async Task<GoogleUserInfoDto?> ValidateGoogleTokenAsync(string accessToken)
    {
        try
        {
            _logger.LogInformation("Validating Google access token");

            // Simple retry policy with Polly v8
            var retryPolicy = Policy
                .Handle<HttpRequestException>()
                .WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    onRetry: (outcome, timespan, retryCount, context) =>
                    {
                        _logger.LogWarning("Retrying Google API call. Attempt: {Attempt}, Delay: {Delay}ms", 
                            retryCount, timespan.TotalMilliseconds);
                    });

            var result = await retryPolicy.ExecuteAsync(async () =>
            {
                var request = new HttpRequestMessage(HttpMethod.Get, 
                    $"https://www.googleapis.com/oauth2/v2/userinfo?access_token={accessToken}");
                
                var response = await _httpClient.SendAsync(request);
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Google API returned status code: {StatusCode}", response.StatusCode);
                    
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        return null; // Invalid token
                    }
                    
                    throw new HttpRequestException($"Google API call failed with status: {response.StatusCode}");
                }

                var jsonContent = await response.Content.ReadAsStringAsync();
                var userInfo = JsonSerializer.Deserialize<GoogleUserInfoDto>(jsonContent, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
                });

                _logger.LogInformation("Successfully retrieved Google user info for email: {Email}", userInfo?.Email);
                return userInfo;
            });

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating Google access token");
            throw new InvalidOperationException("Failed to validate Google access token", ex);
        }
    }
}