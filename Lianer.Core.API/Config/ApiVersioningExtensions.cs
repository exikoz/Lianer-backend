using Asp.Versioning;

public static class ApiVersioningExtensions
{
    public const string GroupNameFormat = "'v'VVV";
    // For example: API version: 1.4 (Major = 1. Minor = 4)
    public const int MajorVersion = 1;
    public const int MinorVersion = 0;
    
    public static IServiceCollection SetupApiVersioning(this IServiceCollection services)
    {
            // API versioning (Asp.Versioning)
            services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(MajorVersion, MinorVersion);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
            })
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = GroupNameFormat;
                options.SubstituteApiVersionInUrl = true;
            });
        return services;
    }
}






