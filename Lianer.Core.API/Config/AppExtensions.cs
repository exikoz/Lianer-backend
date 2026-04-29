using Lianer.Core.API.Filters;
using Lianer.Core.API.Services;

namespace Lianer.Core.API.Config;
public static class AppExtensions
{
    public static IServiceCollection SetupServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IGoogleAuthService, GoogleAuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IActivityService, ActivityService>();
        services.AddScoped<IActivityQueryService, ActivityQueryService>();
        services.AddScoped<INoteService, NoteService>();
        services.AddScoped<INoteQueryService, NoteQueryService>();
        services.AddScoped<IContactService, ContactService>();
        return services;
    }

    public static IServiceCollection SetupRepositories(this IServiceCollection services)
    {
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IActivityRepository, ActivityRepository>();
            services.AddScoped<INoteRepository, NoteRepository>();
            services.AddScoped<IContactRepository, ContactRepository>();
            return services;
    }

    public static IServiceCollection SetupControllers(this IServiceCollection services)
    {
            services.AddControllers(options =>
            {
                // Register custom validation filter globally
                options.Filters.Add<ValidateModelFilter>();
            })
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
            });
            return services;
    }

}

