using FluentAssertions;
using Lianer.Features.API.Clients;
using Lianer.Features.API.DTOs;
using Lianer.Features.API.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using System.Net;
using System.Net.Http.Json;

namespace Lianer.Features.API.Tests.Unit;

/// <summary>
/// Unit tests for HunterService.
/// Mocks HunterClient via a mocked HttpMessageHandler to isolate business logic.
/// </summary>
public class HunterServiceTests
{
    private readonly Mock<ILogger<HunterService>> _loggerMock;
    private readonly Mock<ILogger<HunterClient>> _clientLoggerMock;

    public HunterServiceTests()
    {
        _loggerMock = new Mock<ILogger<HunterService>>();
        _clientLoggerMock = new Mock<ILogger<HunterClient>>();
    }

    private (HunterService service, Mock<HttpMessageHandler> handlerMock) CreateService(
        HttpResponseMessage response)
    {
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        var httpClient = new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri("https://api.hunter.io/")
        };

        var configMock = new Mock<Microsoft.Extensions.Configuration.IConfiguration>();
        configMock.Setup(c => c["Hunter:ApiKey"]).Returns("test-api-key");

        var client = new HunterClient(httpClient, configMock.Object, _clientLoggerMock.Object);
        var service = new HunterService(client, _loggerMock.Object);

        return (service, handlerMock);
    }

    [Fact]
    public async Task EnrichDomainAsync_GiltigDomän_ReturnerarData()
    {
        // Arrange
        var hunterResponse = new HunterResponseDto(
            new HunterDataDto("stripe.com", "Stripe", [
                new HunterEmailDto("john@stripe.com", "personal", 90, "John", "Doe", "Engineer", "Engineering")
            ]),
            new HunterMetaDto(1));

        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(hunterResponse)
        };

        var (service, _) = CreateService(response);

        // Act
        var result = await service.EnrichDomainAsync("stripe.com");

        // Assert
        result.Should().NotBeNull();
        result!.Domain.Should().Be("stripe.com");
        result.Organization.Should().Be("Stripe");
        result.Emails.Should().HaveCount(1);
        result.Emails[0].Value.Should().Be("john@stripe.com");
    }

    [Fact]
    public async Task EnrichDomainAsync_ExterntApiFelaktig_KastarHttpRequestException()
    {
        // Arrange
        var response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
        var (service, _) = CreateService(response);

        // Act
        var act = () => service.EnrichDomainAsync("bad-domain.com");

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>();
    }

    [Fact]
    public async Task EnrichDomainAsync_TomResponse_ReturnerarNull()
    {
        // Arrange
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("null", System.Text.Encoding.UTF8, "application/json")
        };

        var (service, _) = CreateService(response);

        // Act
        var result = await service.EnrichDomainAsync("empty.com");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task FindContactEmailAsync_GiltigPerson_ReturnerarEmail()
    {
        // Arrange
        var finderResponse = new HunterEmailFinderResponseDto(
            new HunterEmailFinderDataDto(
                "anna@example.com", "Anna", "Svensson", "CTO", "Example AB", 95, null));

        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(finderResponse)
        };

        var (service, _) = CreateService(response);

        // Act
        var result = await service.FindContactEmailAsync("example.com", "Anna", "Svensson");

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be("anna@example.com");
        result.FirstName.Should().Be("Anna");
        result.LastName.Should().Be("Svensson");
    }

    [Fact]
    public async Task EnrichDomainAsync_SkickarApiKeyIHeader()
    {
        // Arrange
        HttpRequestMessage? capturedRequest = null;

        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, _) => capturedRequest = req)
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("null", System.Text.Encoding.UTF8, "application/json")
            });

        var httpClient = new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri("https://api.hunter.io/")
        };

        var configMock = new Mock<Microsoft.Extensions.Configuration.IConfiguration>();
        configMock.Setup(c => c["Hunter:ApiKey"]).Returns("my-secret-key");

        var client = new HunterClient(httpClient, configMock.Object, _clientLoggerMock.Object);
        var service = new HunterService(client, _loggerMock.Object);

        // Act
        await service.EnrichDomainAsync("test.com");

        // Assert
        capturedRequest.Should().NotBeNull();
        capturedRequest!.Headers.GetValues("X-API-KEY").Should().Contain("my-secret-key");
    }
}
