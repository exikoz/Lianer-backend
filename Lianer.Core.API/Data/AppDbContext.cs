using Lianer.Core.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Lianer.Core.API.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options)
    : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Activity> Activities => Set<Activity>();
    public DbSet<Note> Notes => Set<Note>();
    public DbSet<Contact> Contacts => Set<Contact>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<User>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.HasIndex(x => x.Email).IsUnique();

            entity.Property(x => x.FirstName)
                .IsRequired()
                .HasMaxLength(50);
            entity.Property(x => x.LastName)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(x => x.Email)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(x => x.PasswordHash)
                .HasMaxLength(255);

            entity.Property(x => x.ExternalProviderId)
                .HasMaxLength(255);

            entity.Property(x => x.Provider)
                .IsRequired()
                .HasMaxLength(50);
        });

        builder.Entity<Note>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Title)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(x => x.Content)
                .IsRequired()
                .HasMaxLength(100);

            entity.HasIndex(x => x.CreatedBy);
        });

        builder.Entity<Activity>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Description)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(x => x.CreatedBy)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(x => x.Status)
                .HasConversion<int>();

            entity.HasIndex(x => x.AssignedTo);
            entity.HasIndex(x => x.Status);

            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(x => x.AssignedTo)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(x => x.Note)
                .WithMany()
                .HasForeignKey(x => x.NoteId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<Contact>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.FirstName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(x => x.LastName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(x => x.Role)
                .HasMaxLength(150);

            entity.Property(x => x.Company)
                .HasMaxLength(150);

            entity.Property(x => x.Status)
                .HasConversion<int>();

            entity.HasIndex(x => x.Company);
            entity.HasIndex(x => x.Status);
            entity.HasIndex(x => x.AssignedTo);
            entity.HasIndex(x => x.IsFavorite);

            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(x => x.AssignedTo)
                .OnDelete(DeleteBehavior.SetNull);

            var listComparer = new ValueComparer<List<string>>(
                (c1, c2) => c1!.SequenceEqual(c2!),
                c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                c => c.ToList());

            entity.Property(x => x.Phone)
                .HasConversion(
                    v => string.Join(';', v),
                    v => string.IsNullOrWhiteSpace(v)
                        ? new List<string>()
                        : v.Split(';', StringSplitOptions.RemoveEmptyEntries).ToList())
                .Metadata.SetValueComparer(listComparer);

            entity.Property(x => x.Email)
                .HasConversion(
                    v => string.Join(';', v),
                    v => string.IsNullOrWhiteSpace(v)
                        ? new List<string>()
                        : v.Split(';', StringSplitOptions.RemoveEmptyEntries).ToList())
                .Metadata.SetValueComparer(listComparer);

            entity.OwnsOne(x => x.Social, social =>
            {
                social.Property(x => x.LinkedIn)
                    .HasMaxLength(500);

                social.Property(x => x.Website)
                    .HasMaxLength(500);
            });

            entity.OwnsMany(x => x.InteractionLog, log =>
            {
                log.WithOwner().HasForeignKey("ContactId");

                log.HasKey(x => x.Id);

                log.Property(x => x.Type)
                    .IsRequired()
                    .HasMaxLength(100);

                log.Property(x => x.Content)
                    .IsRequired()
                    .HasMaxLength(2000);

                log.Property(x => x.PreviousStatus)
                    .HasMaxLength(50);

                log.Property(x => x.NewStatus)
                    .HasMaxLength(50);
            });
        });
    }
}