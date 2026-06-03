namespace EmailSender.Application.Services;

/// <summary>
/// Validators for template field types.
/// </summary>
public static class TemplateFieldValidators
{
    /// <summary>
    /// Validates that a value is a valid email address.
    /// </summary>
    public static bool IsValidEmail(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        try
        {
            var addr = new System.Net.Mail.MailAddress(value);
            return addr.Address == value;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Validates that a value can be parsed as a date.
    /// </summary>
    public static bool IsValidDate(object? value)
    {
        return DateFormattingUtilities.TryParseDate(value, out _);
    }

    /// <summary>
    /// Validates that a value is a valid text string and meets length constraints.
    /// </summary>
    public static bool IsValidText(object? value, int? minLength = null, int? maxLength = null)
    {
        if (value == null)
            return minLength == null || minLength == 0;

        var str = value.ToString() ?? "";

        if (minLength.HasValue && str.Length < minLength.Value)
            return false;

        if (maxLength.HasValue && str.Length > maxLength.Value)
            return false;

        return true;
    }

    /// <summary>
    /// Validates a field value based on its type and optional validation rules.
    /// </summary>
    public static bool ValidateField(string fieldType, object? value, Dictionary<string, object>? validation, out string? errorMessage)
    {
        errorMessage = null;

        // Check required
        if (value == null || string.IsNullOrWhiteSpace(value?.ToString()))
        {
            // Note: This should be checked before calling this, but good to catch here too
            return true; // Empty values are handled by caller as required/not required
        }

        switch (fieldType)
        {
            case "email":
                if (!IsValidEmail(value?.ToString()))
                {
                    errorMessage = "Invalid email format";
                    return false;
                }
                break;

            case "date":
                if (!IsValidDate(value))
                {
                    errorMessage = "Invalid date format";
                    return false;
                }
                break;

            case "text":
                int? minLength = null, maxLength = null;
                if (validation?.TryGetValue("minLength", out var min) == true && min is int minInt)
                    minLength = minInt;
                if (validation?.TryGetValue("maxLength", out var max) == true && max is int maxInt)
                    maxLength = maxInt;

                if (!IsValidText(value, minLength, maxLength))
                {
                    if (minLength.HasValue && maxLength.HasValue)
                        errorMessage = $"Text must be between {minLength} and {maxLength} characters";
                    else if (minLength.HasValue)
                        errorMessage = $"Text must be at least {minLength} characters";
                    else if (maxLength.HasValue)
                        errorMessage = $"Text must not exceed {maxLength} characters";
                    return false;
                }
                break;

            default:
                errorMessage = $"Unknown field type: {fieldType}";
                return false;
        }

        return true;
    }
}
