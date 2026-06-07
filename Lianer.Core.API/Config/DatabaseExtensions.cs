using Lianer.Core.API.Data;
using Microsoft.EntityFrameworkCore;

namespace Lianer.Core.API.Config;

public static class DatabaseExtensions
{

    private const string DatabaseName = "LianerDb";
    private const string TestDatabaseName = "TestDatabase";
    // Database (EF Core InMemory)
    public static IServiceCollection SetupInMemoryDb(this IServiceCollection services,IWebHostEnvironment environment)
    {

        var databaseName = environment.IsEnvironment("Development")
            ? TestDatabaseName
            : DatabaseName;
        Console.BackgroundColor = ConsoleColor.Black;
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Running in: {environment.EnvironmentName}. Database running: {databaseName}");
        services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase(databaseName));
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

