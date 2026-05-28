using EmailSender.Application.Interfaces;
using EmailSender.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EmailSender.Infrastructure.Persistence;

public sealed class EmailSenderDbContext(DbContextOptions<EmailSenderDbContext> options)
    : DbContext(options), IUnitOfWork
{
    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<EmailAttempt> EmailAttempts => Set<EmailAttempt>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.HasKey(user => user.Id);

            entity.HasIndex(user => user.GoogleSubject).IsUnique();
            entity.HasIndex(user => user.Email);

            entity.Property(user => user.GoogleSubject)
                .HasMaxLength(128)
                .IsRequired();

            entity.Property(user => user.Email)
                .HasMaxLength(320)
                .IsRequired();

            entity.Property(user => user.DisplayName)
                .HasMaxLength(200);

            entity.Property(user => user.PictureUrl)
                .HasMaxLength(1000);

            entity.Property(user => user.EncryptedRefreshToken)
                .HasMaxLength(4096);
        });

        modelBuilder.Entity<EmailAttempt>(entity =>
        {
            entity.HasKey(attempt => attempt.Id);

            entity.HasIndex(attempt => new
            {
                attempt.UserId,
                attempt.CreatedAt
            });

            entity.HasIndex(attempt => new
            {
                attempt.Status,
                attempt.ScheduledAt
            });

            entity.Property(attempt => attempt.RecipientEmail)
                .HasMaxLength(320)
                .IsRequired();

            entity.Property(attempt => attempt.Subject)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(attempt => attempt.Message)
                .HasMaxLength(20000)
                .IsRequired();

            entity.Property(attempt => attempt.Status)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(attempt => attempt.ErrorMessage)
                .HasMaxLength(2000);

            entity.HasOne(attempt => attempt.User)
                .WithMany(user => user.EmailAttempts)
                .HasForeignKey(attempt => attempt.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}