using Scalar.AspNetCore;

public static class InDevelopment
{
    // 7. OpenAPI / Scalar (dev only)
    public static WebApplication SetupDevelopment(this WebApplication app, IConfiguration config)
    {
        if(app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference(options =>
            {
            // Denna metod ersätter den föråldrade OAuth2Options
            // Vi skickar med namnet "GoogleAuth" som matchar vår DocumentTransformer
            options.AddAuthorizationCodeFlow("GoogleAuth", auth =>
            {
                auth.ClientId = config["Google:Auth:ClientId"];
                auth.SelectedScopes = ["openid", "email", "profile"];
            });
            });
        }
        return app;
    }
}