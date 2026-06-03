namespace EmailSender.Application.Dtos;

/// <summary>
/// Request to generate a preview of an email from a template.
/// Used by the preview API endpoint.
/// </summary>
public class EmailGenerationRequest
{
    /// <summary>
    /// ID of the template to use (e.g., "delivery-reminder").
    /// </summary>
    public required string TemplateId { get; set; }

    /// <summary>
    /// Field values provided by the user for this template.
    /// Keys are field names from the template schema.
    /// </summary>
    public required Dictionary<string, object?> FieldValues { get; set; }
}

/// <summary>
/// Response containing generated email content (subject and body).
/// Returned by preview API and used internally for scheduled emails.
/// </summary>
public class EmailGenerationResponse
{
    public required string Subject { get; set; }
    public required string Body { get; set; }
}
