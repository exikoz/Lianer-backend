using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Lianer.Core.API.DTOs.Auth;

namespace Lianer.Core.API.Tests.Integration;

/// <summary>
/// Integrationstester för /api/v1/sessions (login).
/// Testar hela HTTP-kedjan med WebApplicationFactory.
/// </summary>
public class SessionsEndpointTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public SessionsEndpointTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
        _factory = factory;
    }

    private async Task RegisterUser(string email, string password)
    {
        var request = new RegisterRequestDto
        {
            FirstName = "Test",
            LastName="User",
            Email = email,
            Password = password
        };
        var response = await _client.PostAsJsonAsync("/api/v1/users", request);
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task POST_Sessions_GiltigaUppgifter_Returnerar200MedToken()
    {
        var email = $"test_{Guid.NewGuid()}@example.com";
        var password = "Secure@Password1";
        await RegisterUser(email, password);

        var request = new LoginRequestDto
        {
            Email = email,
            Password = password
        };

        var response = await _client.PostAsJsonAsync("/api/v1/sessions", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
        body.Should().NotBeNull();
        body!.AccessToken.Should().NotBeNullOrWhiteSpace();
        body.TokenType.Should().Be("Bearer");
        body.ExpiresIn.Should().BeGreaterThan(0);
        body.User.Should().NotBeNull();
        body.User.Email.Should().Be(email);
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
        var email = $"wrongpass_{Guid.NewGuid()}@example.com";
        await RegisterUser(email, "Secure@Password1");

        var request = new LoginRequestDto
        {
            Email = email,
            Password = "Helt@FelLösenord1"
        };

        var response = await _client.PostAsJsonAsync("/api/v1/sessions", request);

        // AuthService kastar UnauthorizedAccessException som fångas av ExceptionMiddleware
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
