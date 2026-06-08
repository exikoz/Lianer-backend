using System.Net.Http.Json;
using Lianer.Features.API.DTOs.Integration;

namespace Lianer.Features.API.Clients;

/// <summary>
/// Typed HttpClient to handle communication with the Core API microservice
/// </summary>
public class CoreApiClient(HttpClient httpClient, ILogger<CoreApiClient> logger)
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly ILogger<CoreApiClient> _logger = logger;

    /// <summary>
    /// Fetches a user summary from the Core API by user ID
    /// </summary>
    /// <param name="userId">The ID of the user to retrieve</param>
    /// <returns>A DTO containing user information, or null if not found</returns>
    public async Task<CoreUserSummaryDto?> GetUserSummaryAsync(Guid userId)
    {
        _logger.LogInformation("Requesting user summary for {UserId} from Core API", userId);

        try
        {
            var response = await _httpClient.GetAsync($"/api/v1/users/{userId}");
            
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning("User {UserId} not found in Core API", userId);
                return null;
            }

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<CoreUserSummaryDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error communicating with Core API for user {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// Fetches all users from the Core API
    /// </summary>
    /// <returns>A list of user summaries</returns>
    public async Task<IEnumerable<CoreUserSummaryDto>> GetUsersAsync()
    {
        _logger.LogInformation("Requesting all users from Core API");

        try
        {
            var response = await _httpClient.GetAsync("/api/v1/users");
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<IEnumerable<CoreUserSummaryDto>>() ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error communicating with Core API to list users");
            throw;
        }
    }
}
