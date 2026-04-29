using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

public static class RateLimitExtensions
{
    public const string FixedWindowPolicy = "fixed";
    public const int PermitLimit = 100;
    public const int TimeSpanMinutes = 1;
    public const int QueueLimit = 10;

    public static IServiceCollection SetupRateLimiting(this IServiceCollection services)
    {
      // Configure Rate Limiting — Fixed Window
            services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

                options.AddFixedWindowLimiter(FixedWindowPolicy, limiterOptions =>
                {
                    limiterOptions.PermitLimit = PermitLimit;
                    limiterOptions.Window = TimeSpan.FromMinutes(TimeSpanMinutes);
                    limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    limiterOptions.QueueLimit = QueueLimit;
                });
            });

        return services;
    }
}