using EmailSender.Application.Dtos;
using EmailSender.Application.Interfaces;

namespace EmailSender.Application.Services;

/// <summary>
/// Registry for managing available email templates.
/// </summary>
public sealed class TemplateRegistry : ITemplateRegistry
{
    private readonly Dictionary<string, IEmailTemplate> _templates = new();

    public TemplateRegistry()
    {
        // Register built-in templates
        RegisterTemplate(new DeliveryReminderTemplate());
    }

    public void RegisterTemplate(IEmailTemplate template)
    {
        _templates[template.TemplateId] = template;
    }

    public IEmailTemplate? GetTemplate(string templateId)
    {
        _templates.TryGetValue(templateId, out var template);
        return template;
    }

    public IReadOnlyList<TemplateMetadata> ListTemplates()
    {
        return _templates.Values
            .Select(t => new TemplateMetadata
            {
                Id = t.TemplateId,
                Name = t.Name,
                Description = t.Description
            })
            .ToList();
    }
}
