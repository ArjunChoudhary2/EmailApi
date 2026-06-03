using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EmailSender.Infrastructure;

/// <summary>
/// Configuration for timezone handling in email scheduling.
/// </summary>
public class TimezoneConfiguration
{
    public const string DefaultTimezone = "Asia/Kolkata"; // IST
    public const string DefaultTimezoneOffset = "+05:30";

    /// <summary>
    /// The system's default timezone (IST - Indian Standard Time).
    /// </summary>
    public static TimeZoneInfo IstTimeZone { get; } = TimeZoneInfo.FindSystemTimeZoneById("Asia/Kolkata");

    /// <summary>
    /// Converts UTC datetime to IST.
    /// </summary>
    public static DateTime ConvertUtcToIst(DateTime utcDateTime)
    {
        return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, IstTimeZone);
    }

    /// <summary>
    /// Converts IST datetime to UTC.
    /// </summary>
    public static DateTime ConvertIstToUtc(DateTime istDateTime)
    {
        var istTime = DateTime.SpecifyKind(istDateTime, DateTimeKind.Unspecified);
        return TimeZoneInfo.ConvertTimeToUtc(istTime, IstTimeZone);
    }

    /// <summary>
    /// Converts DateTimeOffset (with any offset) to UTC.
    /// </summary>
    public static DateTime ConvertToUtc(DateTimeOffset dateTimeOffset)
    {
        return dateTimeOffset.UtcDateTime;
    }

    /// <summary>
    /// Creates a DateTimeOffset in IST from a DateTime.
    /// </summary>
    public static DateTimeOffset CreateIstDateTime(DateTime dateTime)
    {
        // Assume the input datetime is in IST (unspecified kind)
        var istTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Unspecified);
        var istOffset = TimeSpan.FromHours(5).Add(TimeSpan.FromMinutes(30));
        return new DateTimeOffset(istTime, istOffset);
    }

    /// <summary>
    /// Gets the IST offset (+05:30) as a TimeSpan.
    /// </summary>
    public static TimeSpan IstOffset => TimeSpan.FromHours(5).Add(TimeSpan.FromMinutes(30));
}

/// <summary>
/// Extension methods for adding timezone configuration to DI.
/// </summary>
public static class TimezoneConfigurationExtensions
{
    public static IServiceCollection AddTimezoneConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        // Timezone configuration is static and doesn't need DI registration
        return services;
    }
}
