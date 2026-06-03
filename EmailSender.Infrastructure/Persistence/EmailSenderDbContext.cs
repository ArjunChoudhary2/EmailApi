using EmailSender.Application.Interfaces;
using EmailSender.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EmailSender.Infrastructure.Persistence;

public sealed class EmailSenderDbContext(DbContextOptions<EmailSenderDbContext> options)
    : DbContext(options), IUnitOfWork
{
    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<EmailAttempt> EmailAttempts => Set<EmailAttempt>();
    public DbSet<EmailTemplate> EmailTemplates => Set<EmailTemplate>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EmailTemplate>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.HasIndex(x => new
            {
                x.UserId,
                x.TemplateName
            });

            entity.Property(x => x.TemplateName)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(x => x.SubjectTemplate)
                .HasMaxLength(500)
                .IsRequired();

            entity.Property(x => x.BodyTemplate)
                .HasMaxLength(20000)
                .IsRequired();

            entity.Property(x => x.Description)
                .HasMaxLength(1000);

            entity.Property(x => x.FieldsJson)
                .HasColumnType("jsonb");

            entity.Property(x => x.ReminderOffsetsJson)
                .IsRequired();

            entity.Property(x => x.SchedulingOffsetDays)
                .HasDefaultValue(0);

            entity.Property(x => x.SchedulingTargetField)
                .HasMaxLength(100)
                .HasDefaultValue(string.Empty);

            entity.HasOne(x => x.User)
                .WithMany(x => x.EmailTemplates)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

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
            
            entity.Property(attempt => attempt.TemplateId)
                .HasMaxLength(100);
            
            entity.Property(attempt => attempt.TemplateFieldValuesJson)
                .HasColumnType("jsonb");

            entity.HasOne(attempt => attempt.User)
                .WithMany(user => user.EmailAttempts)
                .HasForeignKey(attempt => attempt.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}