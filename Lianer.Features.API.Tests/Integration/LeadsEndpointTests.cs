using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Lianer.Features.API.Data;
using Lianer.Features.API.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Lianer.Features.API.Tests.Integration;

/// <summary>
/// Integration tests for /api/v1/leads endpoints.
/// Tests the full HTTP pipeline using WebApplicationFactory.
/// </summary>
public class LeadsEndpointTests : IClassFixture<FeaturesWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly FeaturesWebApplicationFactory _factory;

    public LeadsEndpointTests(FeaturesWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task SeedLeads(int count = 3)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<FeaturesDbContext>();

        for (int i = 1; i <= count; i++)
        {
            db.Contacts.Add(new Contact
            {
                FirstName = $"Lead{i}",
                LastName = $"Testsson{i}",
                Email = $"lead{i}_{Guid.NewGuid():N}@test.com",
                Organization = i % 2 == 0 ? "Acme Corp" : "Globex Inc",
                Position = "Developer",
                Source = "Test"
            });
        }

        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task GET_Leads_ReturnerarPagineradLista()
    {
        await SeedLeads(5);

        var response = await _client.GetAsync("/api/v1/leads?page=1&pageSize=2");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        json.GetProperty("page").GetInt32().Should().Be(1);
        json.GetProperty("pageSize").GetInt32().Should().Be(2);
        json.GetProperty("totalCount").GetInt32().Should().BeGreaterThanOrEqualTo(5);
        json.GetProperty("totalPages").GetInt32().Should().BeGreaterThanOrEqualTo(3);
        json.GetProperty("data").GetArrayLength().Should().Be(2);
    }

    [Fact]
    public async Task GET_Leads_StandardPaginering_Returnerar200()
    {
        var response = await _client.GetAsync("/api/v1/leads");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        json.GetProperty("page").GetInt32().Should().Be(1);
        json.GetProperty("pageSize").GetInt32().Should().Be(20);
    }

    [Fact]
    public async Task GET_Leads_SökFiltrering_FiltrerarResultat()
    {
        // Seed specific data
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<FeaturesDbContext>();
            db.Contacts.Add(new Contact
            {
                FirstName = "UniqueSearchName",
                LastName = "Findme",
                Email = $"unique_{Guid.NewGuid():N}@search.com",
                Organization = "SearchOrg",
                Position = "Tester",
                Source = "Test"
            });
            await db.SaveChangesAsync();
        }

        var response = await _client.GetAsync("/api/v1/leads?search=UniqueSearchName");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        json.GetProperty("totalCount").GetInt32().Should().BeGreaterThanOrEqualTo(1);

        var data = json.GetProperty("data");
        data.EnumerateArray().Should().Contain(item =>
            item.GetProperty("firstName").GetString() == "UniqueSearchName");
    }

    [Fact]
    public async Task GET_LeadDetails_OkändId_Returnerar404()
    {
        var response = await _client.GetAsync($"/api/v1/leads/{Guid.NewGuid()}/details");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GET_LeadDetails_GiltigId_Returnerar200()
    {
        Guid leadId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<FeaturesDbContext>();
            var contact = new Contact
            {
                FirstName = "Detail",
                LastName = "Test",
                Email = $"detail_{Guid.NewGuid():N}@test.com",
                Organization = "DetailOrg",
                Position = "QA",
                Source = "Test"
            };
            db.Contacts.Add(contact);
            await db.SaveChangesAsync();
            leadId = contact.Id;
        }

        var response = await _client.GetAsync($"/api/v1/leads/{leadId}/details");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        json.GetProperty("firstName").GetString().Should().Be("Detail");
        json.GetProperty("lastName").GetString().Should().Be("Test");
    }

    [Fact]
    public async Task GET_EnrichLead_GiltigDomän_Returnerar200()
    {
        var response = await _client.GetAsync("/api/v1/leads/enrich/stripe.com");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        json.GetProperty("domain").GetString().Should().Be("stripe.com");
        json.GetProperty("emails").GetArrayLength().Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GET_EnrichLead_TomDomän_Returnerar404()
    {
        var response = await _client.GetAsync("/api/v1/leads/enrich/empty.com");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task POST_Import_UtanAuth_Returnerar401()
    {
        var response = await _client.PostAsync("/api/v1/leads/import/stripe.com", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
