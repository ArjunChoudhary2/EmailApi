using EmailSender.Application.Dtos;
using EmailSender.Application.Interfaces;
using EmailSender.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmailSender.Api.Controllers;

/// <summary>
/// API endpoints for email templates and template-based email scheduling.
/// </summary>
[ApiController]
[Authorize]
[Route("api/templates")]
public sealed class TemplatesController(
    ITemplateRegistry templateRegistry,
    EmailGenerationService emailGenerationService) : ControllerBase
{
    /// <summary>
    /// List all available email templates.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<TemplateMetadata>), StatusCodes.Status200OK)]
    public ActionResult<IEnumerable<TemplateMetadata>> ListTemplates()
    {
        var templates = templateRegistry.ListTemplates();
        return Ok(templates);
    }

    /// <summary>
    /// Get the schema (required fields) for a specific template.
    /// </summary>
    [HttpGet("{templateId}/schema")]
    [ProducesResponseType(typeof(TemplateSchemaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<TemplateSchemaResponse> GetTemplateSchema(string templateId)
    {
        var template = templateRegistry.GetTemplate(templateId);
        if (template == null)
        {
            return NotFound(new { error = $"Template not found: {templateId}" });
        }

        var schema = template.GetSchema();
        var response = new TemplateSchemaResponse
        {
            TemplateId = template.TemplateId,
            Name = template.Name,
            Description = template.Description,
            Fields = schema.Fields,
            SchedulingOffsetDays = schema.SchedulingOffsetDays,
            SchedulingTargetField = schema.SchedulingTargetField
        };

        return Ok(response);
    }
}

/// <summary>
/// Response containing template schema information.
/// </summary>
public class TemplateSchemaResponse
{
    public required string TemplateId { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required List<TemplateField> Fields { get; set; }
    
    /// <summary>
    /// Number of days before the target field date to schedule the email.
    /// If null or 0, no automatic scheduling is configured.
    /// </summary>
    public int? SchedulingOffsetDays { get; set; }
    
    /// <summary>
    /// The field name used as the target date for scheduling (e.g., "deliveryDate").
    /// </summary>
    public string? SchedulingTargetField { get; set; }
}
