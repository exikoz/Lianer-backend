using System.IdentityModel.Tokens.Jwt;
using FluentAssertions;
using Lianer.Core.API.Models;
using Lianer.Core.API.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace Lianer.Core.API.Tests.Unit;

/// <summary>
/// Enhetstester för TokenService.
/// Testar JWT-generering med mockad konfiguration.
/// </summary>
public class TokenServiceTests
{
    private readonly TokenService _sut;
    private readonly IConfiguration _configuration;

    public TokenServiceTests()
    {
        var configData = new Dictionary<string, string?>
        {
            { "JwtSettings:SecretKey", "TestSecretKeyMinstTrettioTvåTeckenLång!!" },
            { "JwtSettings:Issuer", "TestIssuer" },
            { "JwtSettings:Audience", "TestAudience" },
            { "JwtSettings:ExpirationMinutes", "30" }
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        var loggerMock = new Mock<ILogger<TokenService>>();
        _sut = new TokenService(_configuration, loggerMock.Object);
    }

    [Fact]
    public void GenerateAccessToken_ReturnerarGiltigJwtSträng()
    {
        var user = new User("Test","Testsson", "test@example.com", "hash");

        var token = _sut.GenerateAccessToken(user);

        token.Should().NotBeNullOrWhiteSpace();
        token.Split('.').Should().HaveCount(3, "en JWT ska ha tre delar separerade med punkt");
    }

    [Fact]
    public void GenerateAccessToken_InnehållerKorrektaClaims()
    {
        var user = new User("Anna","Svensson", "anna@example.com", "hash");

        var token = _sut.GenerateAccessToken(user);

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        jwt.Claims.Should().Contain(c => c.Type == "email" && c.Value == "anna@example.com");
        jwt.Claims.Should().Contain(c => c.Type == "fullName" && c.Value == "Anna Svensson");
        jwt.Issuer.Should().Be("TestIssuer");
        jwt.Audiences.Should().Contain("TestAudience");
    }

    [Fact]
    public void GetTokenExpirationSeconds_ReturnerarKonfigureratVärde()
    {
        var result = _sut.GetTokenExpirationSeconds();

        result.Should().Be(1800, "30 minuter * 60 sekunder = 1800");
    }

    [Fact]
    public void GenerateAccessToken_TokenHarKorrektUtgångstid()
    {
        var user = new User("Test","Testsson", "test@example.com", "hash");

        var token = _sut.GenerateAccessToken(user);

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        jwt.ValidTo.Should().BeCloseTo(DateTime.UtcNow.AddMinutes(30), TimeSpan.FromSeconds(5));
    }
}
