using Lianer.Core.API.Data;
using Microsoft.EntityFrameworkCore;

namespace Lianer.Core.API.Config;

public static class DatabaseExtensions
{

    private const string DatabaseName = "LianerDb";
    // Database (EF Core InMemory)
    public static IServiceCollection SetupInMemoryDb(this IServiceCollection services)
    {
        services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase(DatabaseName));
        return services;
    }
    public static void InitDatabase(this WebApplication app)
    {
        // Ensure database is created
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.EnsureCreated();
    }
    
}

