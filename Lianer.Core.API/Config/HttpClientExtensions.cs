public static class HttpClientExtensions
{
    public static IServiceCollection SetupGoogleAuth(this IServiceCollection services)
    {
        services.AddHttpClient("GoogleAuth", client =>
            {
                client.BaseAddress = new Uri("https://www.googleapis.com/");
                client.Timeout = TimeSpan.FromSeconds(30);
                client.DefaultRequestHeaders.Add("User-Agent", "Lianer-Backend/1.0");
            });
        return services;
    }

    /// <summary>
    /// Registers a named HttpClient for the Gemini AI API with a 30-second timeout.
    /// The API key is injected per-request (not in the base client) to avoid leaking it in logs.
    /// </summary>
    public static IServiceCollection SetupGeminiClient(this IServiceCollection services)
    {
        services.AddHttpClient("Gemini", client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.Add("User-Agent", "Lianer-Backend/1.0");
        });
        return services;
    }
}