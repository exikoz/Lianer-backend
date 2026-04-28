public static class CorsExtensions
{
    public const string DefaultPolicyName = "DefaultPolicy";

    public static IServiceCollection SetupCorsPolicy(this IServiceCollection services, IConfiguration config)
    {
            // Configure CORS — strict policy, no AllowAnyOrigin
            services.AddCors(options =>
            {
                options.AddPolicy("DefaultPolicy", policy =>
                {
                    policy.WithOrigins(
                            config.GetSection("Cors:AllowedOrigins").Get<string[]>()
                            ?? ["http://localhost:5173", "http://localhost:3000"])
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
            });

        return services;
    }
}