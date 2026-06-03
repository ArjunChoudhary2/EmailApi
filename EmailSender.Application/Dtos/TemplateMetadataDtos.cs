namespace EmailSender.Application.Dtos;

/// <summary>
/// Metadata about an available email template.
/// Used when listing templates to users.
/// </summary>
public class TemplateMetadata
{
    /// <summary>
    /// Unique identifier for this template type.
    /// </summary>
    public required string Id { get; set; }

    /// <summary>
    /// Human-readable name of the template.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Brief description of what this template does.
    /// </summary>
    public string? Description { get; set; }
}
