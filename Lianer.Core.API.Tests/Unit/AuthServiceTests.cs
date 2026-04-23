using FluentAssertions;
using Lianer.Core.API.Data;
using Lianer.Core.API.DTOs.Auth;
using Lianer.Core.API.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace Lianer.Core.API.Tests.Unit;

/// <summary>
/// Enhetstester för AuthService.
/// Mockar ILogger och ITokenService för att isolera affärslogiken.
/// </summary>
public class AuthServiceTests
{
    private readonly Mock<ILogger<AuthService>> _loggerMock;
    private readonly Mock<ITokenService> _tokenServiceMock;
    private readonly AppDbContext _context;
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        _loggerMock = new Mock<ILogger<AuthService>>();
        _tokenServiceMock = new Mock<ITokenService>();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);

        _tokenServiceMock
            .Setup(t => t.GenerateAccessToken(It.IsAny<Models.User>()))
            .Returns("test-jwt-token");

        _tokenServiceMock
            .Setup(t => t.GetTokenExpirationSeconds())
            .Returns(3600);

        _sut = new AuthService(_context, _loggerMock.Object, _tokenServiceMock.Object);
    }

    [Fact]
    public async Task RegisterAsync_GiltigRequest_ReturnerarRegisterResponse()
    {
        var request = new RegisterRequestDto
        {
            FullName = "Test Testsson",
            Email = "test@example.com",
            Password = "SecurePassword123!"
        };

        var result = await _sut.RegisterAsync(request);

        result.Should().NotBeNull();
        result.Email.Should().Be("test@example.com");
        result.FullName.Should().Be("Test Testsson");
        result.UserId.Should().NotBeEmpty();
    }

    [Fact]
    public async Task RegisterAsync_SätterKorrektFörnamnOchEfternamn()
    {
        var request = new RegisterRequestDto
        {
            FullName = "Anna Svensson",
            Email = "anna@example.com",
            Password = "SecurePassword123!"
        };

        var result = await _sut.RegisterAsync(request);

        result.FullName.Should().Be("Anna Svensson");
    }

    [Fact]
    public async Task LoginAsync_GiltigtLösenord_ReturnerarToken()
    {
        // Seed user
        var user = new Models.User("Test", "User", "test@example.com", BCrypt.Net.BCrypt.HashPassword("password123"));
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var request = new LoginRequestDto
        {
            Email = "test@example.com",
            Password = "password123"
        };

        var result = await _sut.LoginAsync(request);

        result.Should().NotBeNull();
        result.AccessToken.Should().Be("test-jwt-token");
        result.TokenType.Should().Be("Bearer");
        result.ExpiresIn.Should().Be(3600);
        result.User.Email.Should().Be("test@example.com");
    }

    [Fact]
    public async Task LoginAsync_FelaktigtLösenord_KastarUnauthorized()
    {
        // Seed user
        var user = new Models.User("Test", "User", "test@example.com", BCrypt.Net.BCrypt.HashPassword("password123"));
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var request = new LoginRequestDto
        {
            Email = "test@example.com",
            Password = "fel-lösenord"
        };

        var act = () => _sut.LoginAsync(request);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Invalid email or password");
    }

    [Fact]
    public async Task GoogleLoginAsync_NyAnvändare_SkaparKontoOchReturnerarToken()
    {
        var googleUser = new GoogleUserInfoDto
        {
            Id = "google-123",
            Name = "Google Användare",
            Email = "google@example.com"
        };

        var result = await _sut.GoogleLoginAsync(googleUser);

        result.Should().NotBeNull();
        result.AccessToken.Should().Be("test-jwt-token");
        result.User.Email.Should().Be("google@example.com");
        result.User.FullName.Should().Be("Google Användare");
    }

    [Fact]
    public async Task RegisterAsync_AnroparTokenServiceFörAttGenereraToken_Aldrig()
    {
        var request = new RegisterRequestDto
        {
            FullName = "Test",
            Email = "test@example.com",
            Password = "Password123!"
        };

        await _sut.RegisterAsync(request);

        _tokenServiceMock.Verify(
            t => t.GenerateAccessToken(It.IsAny<Models.User>()),
            Times.Never,
            "Register ska inte generera en token");
    }

    [Fact]
    public async Task LoginAsync_AnroparTokenServiceEnGång()
    {
        // Seed user
        var user = new Models.User("Test", "User", "test@example.com", BCrypt.Net.BCrypt.HashPassword("password123"));
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var request = new LoginRequestDto
        {
            Email = "test@example.com",
            Password = "password123"
        };

        await _sut.LoginAsync(request);

        _tokenServiceMock.Verify(
            t => t.GenerateAccessToken(It.IsAny<Models.User>()),
            Times.Once,
            "Login ska generera exakt en token");
    }
}
