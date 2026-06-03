namespace EmailSender.Application.Interfaces;

/// <summary>
/// Represents an email template that generates subject and body from field values.
/// Implementations define what fields are required and how to generate the email content.
/// </summary>
public interface IEmailTemplate
{
    /// <summary>
    /// Unique identifier for this template type (e.g., "delivery-reminder").
    /// </summary>
    string TemplateId { get; }

    /// <summary>
    /// Human-readable name of the template.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Brief description of what this template does.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Defines the required input fields for this template and their types.
    /// </summary>
    TemplateSchema GetSchema();

    /// <summary>
    /// Validates that the provided field values meet the schema requirements.
    /// Returns true if valid, false otherwise.
    /// </summary>
    bool ValidateFields(Dictionary<string, object?> fieldValues, out List<string> errors);

    /// <summary>
    /// Generates the email subject and body from the provided field values.
    /// Must throw if fieldValues don't satisfy the schema.
    /// </summary>
    EmailGenerationResult Generate(Dictionary<string, object?> fieldValues);
}

/// <summary>
/// Defines the schema (required fields) for an email template.
/// </summary>
public class TemplateSchema
{
    public List<TemplateField> Fields { get; set; } = new();
    
    /// <summary>
    /// Number of days to schedule the email before the target field date.
    /// If null or 0, no automatic scheduling offset is configured.
    /// </summary>
    public int? SchedulingOffsetDays { get; set; }
    
    /// <summary>
    /// The field name to use as the target date for calculating the scheduled send time.
    /// Example: "deliveryDate" means schedule relative to the deliveryDate field.
    /// </summary>
    public string? SchedulingTargetField { get; set; }
}

/// <summary>
/// Represents a single input field in a template form.
/// </summary>
public class TemplateField
{
    public string Name { get; set; } = null!;
    public string Label { get; set; } = null!;
    public string Type { get; set; } = null!; // "text", "date", "email"
    public bool Required { get; set; } = true;
    public Dictionary<string, object>? Validation { get; set; } // e.g., { "pattern": "...", "minLength": 5 }
}

/// <summary>
/// Result of email generation containing subject and body.
/// </summary>
public class EmailGenerationResult
{
    public string Subject { get; set; } = null!;
    public string Body { get; set; } = null!;
}
