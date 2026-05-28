using EmailSender.Application.Interfaces;
using EmailSender.Application.Services;
using EmailSender.Infrastructure.Google;
using EmailSender.Infrastructure.Persistence;
using EmailSender.Infrastructure.Persistence.Repositories;
using EmailSender.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EmailSender.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? configuration["DATABASE_URL"]
            ?? "Host=localhost;Port=5432;Database=email_sender;Username=postgres;Password=postgres";

        services.AddDbContext<EmailSenderDbContext>(options =>
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorCodesToAdd: null);
    }));

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IEmailAttemptRepository, EmailAttemptRepository>();
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<EmailSenderDbContext>());
        services.AddScoped<ITokenProtector, DataProtectionTokenProtector>();
        services.AddScoped<SchedulerService>();
        services.AddHttpClient<IGoogleOAuthService, GoogleOAuthService>();
        services.AddHttpClient<IGmailSender, GmailSender>();

        return services;
    }
}
