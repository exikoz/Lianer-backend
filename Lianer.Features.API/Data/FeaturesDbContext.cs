using Lianer.Features.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Lianer.Features.API.Data;

/// <summary>
/// Local database context for the Features microservice
/// </summary>
public class FeaturesDbContext(DbContextOptions<FeaturesDbContext> options) : DbContext(options)
{
    /// <summary>
    /// Imported and enriched leads
    /// </summary>
    public DbSet<Contact> Contacts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Ensure email is indexed for faster duplicate checks
        modelBuilder.Entity<Contact>()
            .HasIndex(c => c.Email)
            .IsUnique();
    }
}
