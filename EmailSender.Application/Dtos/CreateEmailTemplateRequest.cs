namespace EmailSender.Api.Controllers
{
    public sealed record EmailTemplateResponseDto(
    Guid Id,
    string TemplateName,
    string SubjectTemplate,
    string BodyTemplate,
    string? Description,
    List<TemplateFieldDto> Fields,
    List<int> ReminderOffsets,
    bool IsSystemTemplate,
    DateTimeOffset CreatedAt);
}