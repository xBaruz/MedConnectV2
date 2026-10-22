using Microsoft.EntityFrameworkCore;
using MedConnect.Models;

namespace MedConnect.Infrastructure.Database;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Appointment> Appointments => Set<Appointment>();

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

            builder.Property(u => u.Role)
                    .HasConversion(
                        role => RoleNames.ToName(role),
                        name => RoleNames.FromName(name)
                    )
                    .HasMaxLength(50)
                    .IsRequired();
        });

        modelBuilder.Entity<RefreshToken>(builder =>
        {
            builder.HasKey(token => token.Id);

            builder.HasIndex(token => token.TokenHash)
                .IsUnique();

            builder.Property(token => token.TokenHash)
                .HasMaxLength(64)
                .IsRequired();

            builder.Property(token => token.UserId)
                .IsRequired();

            builder.Property(token => token.RowVersion)
                .IsRowVersion();

            builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(token => token.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Appointment>(builder =>
        {
            builder.HasKey(a => a.Id);
            builder.Property(a => a.AppointmentDoctorId).IsRequired();
            builder.Property(a => a.AppointmentDate).IsRequired();

            builder.HasOne<User>()
                   .WithMany()
                   .HasForeignKey(a => a.AppointmentDoctorId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<User>()
                   .WithMany()
                   .HasForeignKey(a => a.PatientId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.Property(a => a.Status)
                .HasConversion(
                    status => status.GetType().Name,
                    name => ParseAppointmentStatus(name)
                )
                .HasMaxLength(50)
                .IsRequired();
        });
    }

    private static AppointmentStatus ParseAppointmentStatus(string name) => name switch
    {
        nameof(AppointmentStatus.Scheduled) => new AppointmentStatus.Scheduled(),
        nameof(AppointmentStatus.Canceled) => new AppointmentStatus.Canceled(),
        nameof(AppointmentStatus.Completed) => new AppointmentStatus.Completed(),
        _ => new AppointmentStatus.Available()
    };
}