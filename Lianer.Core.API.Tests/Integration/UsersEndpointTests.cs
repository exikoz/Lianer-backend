using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Lianer.Core.API.DTOs.Auth;

namespace Lianer.Core.API.Tests.Integration;

/// <summary>
/// Integrationstester för /api/v1/users.
/// Testar hela HTTP-kedjan med WebApplicationFactory.
/// </summary>
public class UsersEndpointTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public UsersEndpointTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

[Fact]
public async Task POST_Users_GiltigRequest_Returnerar201()
{
    var request = new RegisterRequestDto
    {
        FirstName = "Test",
        LastName = "Testsson",
        Email = "test@example.com",
        Password = "Secure@Password1"
    };

    var response = await _client.PostAsJsonAsync("/api/v1/users", request);

    response.StatusCode.Should().Be(HttpStatusCode.Created);

    var userId = await response.Content.ReadFromJsonAsync<Guid>();

    userId.Should().NotBeEmpty();
}

    [Fact]
    public async Task POST_Users_SaknarEmail_Returnerar400()
    {
        var request = new { FullName = "Test", Password = "Password123!" };

        var response = await _client.PostAsJsonAsync("/api/v1/users", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task POST_Users_SaknarLösenord_Returnerar400()
    {
        var request = new { FullName = "Test", Email = "test@example.com" };

        var response = await _client.PostAsJsonAsync("/api/v1/users", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GET_Users_OkändId_Returnerar404()
    {
        var response = await _client.GetAsync($"/api/v1/users/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
