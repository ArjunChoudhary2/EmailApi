using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace EmailSender.Infrastructure.Persistence;

public sealed class EmailSenderDbContextFactory : IDesignTimeDbContextFactory<EmailSenderDbContext>
{
    public EmailSenderDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL")
            ?? "Host=localhost;Port=5432;Database=email_sender;Username=postgres;Password=postgres";

        var options = new DbContextOptionsBuilder<EmailSenderDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new EmailSenderDbContext(options);
    }
}
