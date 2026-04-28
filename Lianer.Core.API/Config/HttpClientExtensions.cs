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
}