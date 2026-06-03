using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace EmailSender.Infrastructure.Persistence;

public sealed class EmailSenderDbContextFactory : IDesignTimeDbContextFactory<EmailSenderDbContext>
{
    public EmailSenderDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL")
            ?? "Host=db.zmajymfilpgkocxegrfl.supabase.co;Port=5432;Database=postgres;Username=postgres;Password=3aviihfW1O1yfYyp;SSL Mode=Require;Trust Server Certificate=true";
        

        var options = new DbContextOptionsBuilder<EmailSenderDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new EmailSenderDbContext(options);
    }
}
