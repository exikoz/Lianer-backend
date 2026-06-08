using Lianer.Features.API.DTOs;
using Lianer.Features.API.Services;

namespace Lianer.Features.API.Tests.Integration;

/// <summary>
/// Fake HunterService for integration tests — returns predictable data
/// without calling the real Hunter.io API.
/// </summary>
public class FakeHunterService : IHunterService
{
    public Task<HunterDataDto?> EnrichDomainAsync(string domain)
    {
        if (domain == "empty.com")
            return Task.FromResult<HunterDataDto?>(null);

        var data = new HunterDataDto(domain, $"Org-{domain}", [
            new HunterEmailDto($"contact@{domain}", "generic", 85, "Test", "User", "Developer", "Engineering"),
            new HunterEmailDto($"info@{domain}", "generic", 70, "Info", "Person", "Manager", "Sales")
        ]);

        return Task.FromResult<HunterDataDto?>(data);
    }

    public Task<HunterEmailFinderDataDto?> FindContactEmailAsync(string domain, string firstName, string lastName)
    {
        var data = new HunterEmailFinderDataDto(
            $"{firstName.ToLower()}@{domain}", firstName, lastName, "Developer", $"Org-{domain}", 90, null);

        return Task.FromResult<HunterEmailFinderDataDto?>(data);
    }
}
