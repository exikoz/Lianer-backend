using System.Net;
using System.Net.Http.Json;

namespace Lianer.Features.API.Tests.Integration;

/// <summary>
/// Fake HTTP handler that simulates Core API responses for integration tests.
/// Returns an empty user list so the Features API doesn't fail on inter-service calls.
/// </summary>
public class FakeCoreApiHandler : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var path = request.RequestUri?.AbsolutePath ?? "";

        // GET /api/v1/users — return empty list
        if (path == "/api/v1/users" && request.Method == HttpMethod.Get)
        {
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(Array.Empty<object>())
            });
        }

        // GET /api/v1/users/{id} — return 404
        if (path.StartsWith("/api/v1/users/") && request.Method == HttpMethod.Get)
        {
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
        }

        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
    }
}
