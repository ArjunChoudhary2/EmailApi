using EmailSender.Application.Interfaces;

namespace EmailSender.Application.Services;

/// <summary>
/// Template for delivery reminder emails.
/// Requires: PO Number, PO Date, Delivery Date, Remarks, Recipient Email(s)
/// </summary>
public sealed class DeliveryReminderTemplate : IEmailTemplate
{
    public string TemplateId => "delivery-reminder";
    public string Name => "Delivery Reminder";
    public string Description => "Send a reminder email about upcoming delivery with PO details";

    public TemplateSchema GetSchema()
    {
        return new TemplateSchema
        {
            Fields = new()
            {
                new()
                {
                    Name = "poNumber",
                    Label = "PO Number",
                    Type = "text",
                    Required = true,
                    Validation = new() { { "minLength", 1 }, { "maxLength", 50 } }
                },
                new()
                {
                    Name = "poDate",
                    Label = "PO Date",
                    Type = "date",
                    Required = true
                },
                new()
                {
                    Name = "deliveryDate",
                    Label = "Delivery Date",
                    Type = "date",
                    Required = true
                },
                new()
                {
                    Name = "remarks",
                    Label = "Remarks",
                    Type = "text",
                    Required = true,
                    Validation = new() { { "maxLength", 500 } }
                },
                new()
                {
                    Name = "recipientEmail",
                    Label = "Recipient Email",
                    Type = "email",
                    Required = true
                },
                new()
                {
                    Name="signature",
                    Label="Signature",
                    Type="text",
                    Validation = new() { { "maxLength", 100 } }
                }
            },
            // Email scheduled 3 days before delivery date
            SchedulingOffsetDays = 15,
            SchedulingTargetField = "deliveryDate"
        };
    }

    public bool ValidateFields(Dictionary<string, object?> fieldValues, out List<string> errors)
    {
        errors = new();

        var requiredFields = new[] { "poNumber", "poDate", "deliveryDate", "remarks", "recipientEmail" };
        foreach (var field in requiredFields)
        {
            if (!fieldValues.ContainsKey(field) || fieldValues[field] == null || string.IsNullOrWhiteSpace(fieldValues[field]?.ToString()))
            {
                errors.Add($"Required field '{field}' is missing or empty");
            }
        }

        if (fieldValues.TryGetValue("poNumber", out var poNumber) && poNumber != null)
        {
            var poStr = poNumber.ToString()!;
            if (poStr.Length > 50)
                errors.Add("PO Number must not exceed 50 characters");
        }

        if (fieldValues.TryGetValue("remarks", out var remarks) && remarks != null)
        {
            var remarksStr = remarks.ToString()!;
            if (remarksStr.Length > 500)
                errors.Add("Remarks must not exceed 500 characters");
        }
        if (fieldValues.TryGetValue("signature", out var signature) && signature != null)
        {
            var remarksStr = signature.ToString()!;
            if (remarksStr.Length > 100)
                errors.Add("Remarks must not exceed 500 characters");
        }



        if (fieldValues.TryGetValue("recipientEmail", out var emailObj) &&
     emailObj is IEnumerable<object> emails)
        {
            foreach (var email in emails)
            {
                var emailStr = email?.ToString();

                if (string.IsNullOrWhiteSpace(emailStr) || !IsValidEmail(emailStr))
                {
                    errors.Add($"Invalid email format: {emailStr}");
                }
            }
        }

        if (fieldValues.TryGetValue("poDate", out var poDateObj) && poDateObj != null)
        {
            if (!TryParseDate(poDateObj, out _))
                errors.Add("Invalid PO Date format");
        }

        if (fieldValues.TryGetValue("deliveryDate", out var deliveryDateObj) && deliveryDateObj != null)
        {
            if (!TryParseDate(deliveryDateObj, out _))
                errors.Add("Invalid Delivery Date format");
        }

        return errors.Count == 0;
    }

    public EmailGenerationResult Generate(Dictionary<string, object?> fieldValues)
    {
        // Validate first
        if (!ValidateFields(fieldValues, out var errors))
            throw new ArgumentException($"Invalid field values: {string.Join(", ", errors)}");

        var poNumber = fieldValues["poNumber"]?.ToString() ?? "";
        var remarks = fieldValues["remarks"]?.ToString() ?? "";
        var signature = fieldValues["signature"]?.ToString() ?? "";
        // Parse dates using shared utility
        DateFormattingUtilities.TryParseDate(fieldValues["poDate"], out var poDate);
        DateFormattingUtilities.TryParseDate(fieldValues["deliveryDate"], out var deliveryDate);

        var subject = $"Delivery Reminder : {poNumber}";

        var body = $"""
            Dear Team,

            This is a reminder that delivery extension is required as delivery date is less than 15 days now.

            PO Number :
            {poNumber}

            PO Date :
            {DateFormattingUtilities.FormatDateDayMonthYear(poDate)}

            Delivery Date :
            {DateFormattingUtilities.FormatDateDayMonthYear(deliveryDate)}

            Remarks :
            {remarks}

            DELIVERY EXTENSION REQUIRED 
            Please feel free to reach out if you have any questions or need to make any changes.

            Best regards,
            {signature}
            """;

        return new EmailGenerationResult
        {
            Subject = subject,
            Body = body
        };
    }

    private static bool TryParseDate(object? dateObj, out DateTime date)
    {
        return DateFormattingUtilities.TryParseDate(dateObj, out date);
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}
