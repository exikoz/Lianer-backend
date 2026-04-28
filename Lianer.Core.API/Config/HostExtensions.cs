public static class HostExtensions
{
    public static IHostBuilder SetupValidation(this IHostBuilder host)
    {
        return host.UseDefaultServiceProvider(options =>
        {
            options.ValidateScopes = true;
            options.ValidateOnBuild = true;
        });
    }
}