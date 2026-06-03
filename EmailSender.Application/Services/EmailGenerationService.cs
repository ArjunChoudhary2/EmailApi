using EmailSender.Application.Dtos;
using EmailSender.Application.Interfaces;

namespace EmailSender.Application.Services;

/// <summary>
/// Service for generating email content from templates.
/// Used by both the preview API and the email scheduler to ensure consistency.
/// </summary>
public sealed class EmailGenerationService(ITemplateRegistry templateRegistry)
{
    /// <summary>
    /// Generates email content (subject and body) from a template.
    /// </summary>
    /// <param name="templateId">The template ID to use.</param>
    /// <param name="fieldValues">The field values to use for generation.</param>
    /// <returns>Generated subject and body.</returns>
    /// <exception cref="ArgumentException">If template not found or field values are invalid.</exception>
    public EmailGenerationResponse Generate(string templateId, Dictionary<string, object?> fieldValues)
    {
        var template = templateRegistry.GetTemplate(templateId)
            ?? throw new ArgumentException($"Template not found: {templateId}");

        if (!template.ValidateFields(fieldValues, out var errors))
        {
            throw new ArgumentException($"Invalid field values: {string.Join(", ", errors)}");
        }

        var result = template.Generate(fieldValues);
        return new EmailGenerationResponse
        {
            Subject = result.Subject,
            Body = result.Body
        };
    }

    /// <summary>
    /// Validates that field values are correct for the given template.
    /// </summary>
    public bool ValidateFields(string templateId, Dictionary<string, object?> fieldValues, out List<string> errors)
    {
        errors = new List<string>();

        var template = templateRegistry.GetTemplate(templateId);
        if (template == null)
        {
            errors.Add($"Template not found: {templateId}");
            return false;
        }

        return template.ValidateFields(fieldValues, out errors);
    }
}
