using Lianer.Core.API.Services;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
     
        services.AddScoped<IActivityRepository, ActivityRepository>();
        services.AddScoped<IActivityService, ActivityService>();
        services.AddScoped<IActivityQueryService, ActivityQueryService>();
 
        services.AddScoped<IContactRepository, ContactRepository>();
        services.AddScoped<IContactService, ContactService>();
 
        services.AddScoped<INoteRepository, NoteRepository>();
        services.AddScoped<INoteService, NoteService>();
        services.AddScoped<INoteQueryService, NoteQueryService>();
 
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserService, UserService>();


      
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IGoogleAuthService, GoogleAuthService>();
        services.AddScoped<ITokenService, TokenService>();


        return services;
    }
}