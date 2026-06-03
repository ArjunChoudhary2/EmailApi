using System.Globalization;
using System.Text.Json;

namespace EmailSender.Application.Services;

/// <summary>
/// Utilities for formatting dates in email templates.
/// </summary>
public static class DateFormattingUtilities
{
    /// <summary>
    /// Formats a date in "dd MMM yyyy" format (e.g., "25 May 2026").
    /// </summary>
    public static string FormatDateDayMonthYear(DateTime date)
    {
        return date.ToString("dd MMM yyyy", CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Formats a date in "dd MMM yyyy" format (e.g., "25 May 2026").
    /// </summary>
    public static string FormatDateDayMonthYear(DateTimeOffset date)
    {
        return date.DateTime.ToString("dd MMM yyyy", CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Formats a date in ISO 8601 format (e.g., "2026-05-25").
    /// </summary>
    public static string FormatDateISO(DateTime date)
    {
        return date.ToString("yyyy-MM-dd");
    }

    /// <summary>
    /// Formats a date in ISO 8601 format with time (e.g., "2026-05-25T15:30:00").
    /// </summary>
    public static string FormatDateTimeISO(DateTime dateTime)
    {
        return dateTime.ToString("yyyy-MM-ddTHH:mm:ss");
    }

    /// <summary>
    /// Tries to parse a date from various common formats.
    /// </summary>
    public static bool TryParseDate(object? dateObj, out DateTime date)
    {
        date = default;

        if (dateObj == null)
            return false;

        if (dateObj is DateTime dt)
        {
            date = dt;
            return true;
        }

        if (dateObj is DateTimeOffset dto)
        {
            date = dto.DateTime;
            return true;
        }

        if (dateObj is string str)
        {
            // Trim whitespace from deserialized JSON strings
            str = str.Trim();
            
            // Try parsing common formats: "yyyy-MM-dd", "yyyy-MM-dd HH:mm:ss", ISO 8601, etc.
            var formats = new[]
            {
                "yyyy-MM-dd",
                "yyyy-MM-dd HH:mm:ss",
                "yyyy-MM-ddTHH:mm:ss",
                "yyyy-MM-ddTHH:mm:ssZ",
                "dd/MM/yyyy",
                "dd-MM-yyyy",
                "MM/dd/yyyy",
                "MM-dd-yyyy"
            };

            return DateTime.TryParseExact(str, formats, CultureInfo.InvariantCulture, 
                System.Globalization.DateTimeStyles.None, out date)
                || DateTime.TryParse(str, CultureInfo.InvariantCulture, 
                System.Globalization.DateTimeStyles.None, out date);
        }

        // Handle JsonElement from JSON deserialization
        if (dateObj is JsonElement jsonElement)
        {
            if (jsonElement.ValueKind == JsonValueKind.String)
            {
                var dateStr = jsonElement.GetString()?.Trim() ?? string.Empty;
                if (string.IsNullOrEmpty(dateStr))
                    return false;

                var formats = new[]
                {
                    "yyyy-MM-dd",
                    "yyyy-MM-dd HH:mm:ss",
                    "yyyy-MM-ddTHH:mm:ss",
                    "yyyy-MM-ddTHH:mm:ssZ",
                    "dd/MM/yyyy",
                    "dd-MM-yyyy",
                    "MM/dd/yyyy",
                    "MM-dd-yyyy"
                };

                return DateTime.TryParseExact(dateStr, formats, CultureInfo.InvariantCulture, 
                    System.Globalization.DateTimeStyles.None, out date)
                    || DateTime.TryParse(dateStr, CultureInfo.InvariantCulture, 
                    System.Globalization.DateTimeStyles.None, out date);
            }
        }

        return false;
    }
}
