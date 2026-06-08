using Microsoft.ApplicationInsights.AspNetCore.Extensions;

namespace Lianer.Features.API.Config;

public static class TelemetryExtensions
{
    public static IServiceCollection SetupTelemetry(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"];
        
        if (!string.IsNullOrEmpty(connectionString))
        {
            var options = new ApplicationInsightsServiceOptions
            {
                ConnectionString = connectionString
            };
            services.AddApplicationInsightsTelemetry(options);
        }
        else
        {
            Console.WriteLine("Features API Telemetry: APPLICATIONINSIGHTS_CONNECTION_STRING is missing. Telemetry is disabled.");
        }

        return services;
    }
}
