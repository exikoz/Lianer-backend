using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Lianer.Core.API.DTOs.Auth;

namespace Lianer.Core.API.Tests.Integration;

/// <summary>
/// Integrationstester för /api/v1/sessions (login).
/// Testar hela HTTP-kedjan med WebApplicationFactory.
/// </summary>
public class SessionsEndpointTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public SessionsEndpointTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task POST_Sessions_GiltigaUppgifter_Returnerar200MedToken()
    {
        var request = new LoginRequestDto
        {
            Email = "test@example.com",
            Password = "password123"
        };

        var response = await _client.PostAsJsonAsync("/api/v1/sessions", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
        body.Should().NotBeNull();
        body!.AccessToken.Should().NotBeNullOrWhiteSpace();
        body.TokenType.Should().Be("Bearer");
        body.ExpiresIn.Should().BeGreaterThan(0);
        body.User.Should().NotBeNull();
        body.User.Email.Should().Be("test@example.com");
    }

    [Fact]
    public async Task POST_Sessions_TomBody_Returnerar400()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/sessions", new { });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task POST_Sessions_FelLösenord_Returnerar401()
    {
        var request = new LoginRequestDto
        {
            Email = "test@example.com",
            Password = "helt-fel-lösenord"
        };

        var response = await _client.PostAsJsonAsync("/api/v1/sessions", request);

        // AuthService kastar UnauthorizedAccessException som fångas av ExceptionMiddleware
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
