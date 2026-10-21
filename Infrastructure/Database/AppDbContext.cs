using Microsoft.EntityFrameworkCore;
using MedConnect.Models;

namespace MedConnect.Infrastructure.Database;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(builder =>
        {
            builder.HasKey(u => u.Id);
            builder.HasIndex(u => u.Email).IsUnique();
            builder.Property(u => u.Email).IsRequired().HasMaxLength(256);
            builder.Property(u => u.Password).IsRequired();
            builder.Property(u => u.Firstname).IsRequired().HasMaxLength(100);
            builder.Property(u => u.Lastname).IsRequired().HasMaxLength(100);
            builder.Property(u => u.DateBirth).IsRequired();
        });

        modelBuilder.Entity<RefreshToken>(builder =>
        {
            builder.HasKey(rt => rt.Id);
            builder.HasIndex(rt => rt.Token).IsUnique();
            builder.Property(rt => rt.Token).IsRequired().HasMaxLength(200);
            builder.Property(rt => rt.UserId).IsRequired();
            
            builder.HasOne<User>()
                   .WithMany()
                   .HasForeignKey(rt => rt.UserId)
                   .OnDelete(DeleteBehavior.Cascade);
        });
    }
}