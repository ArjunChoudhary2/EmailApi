using EmailSender.Application.Dtos;

namespace EmailSender.Application.Interfaces;

/// <summary>
/// Registry for managing available email templates.
/// Allows looking up templates by ID and listing all available templates.
/// </summary>
public interface ITemplateRegistry
{
    /// <summary>
    /// Gets a template by its ID.
    /// </summary>
    /// <param name="templateId">The template ID (e.g., "delivery-reminder").</param>
    /// <returns>The template implementation, or null if not found.</returns>
    IEmailTemplate? GetTemplate(string templateId);

    /// <summary>
    /// Lists all available templates with their metadata.
    /// </summary>
    IReadOnlyList<TemplateMetadata> ListTemplates();

    /// <summary>
    /// Registers a template implementation.
    /// </summary>
    void RegisterTemplate(IEmailTemplate template);
}
