using System.ComponentModel.DataAnnotations;

namespace EmailSender.Api.Controllers
{
    public class CreateEmailTemplateRequest
    {
        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string TemplateName { get; set; } = string.Empty;

        [Required]
        [StringLength(500, MinimumLength = 1)]
        public string SubjectTemplate { get; set; } = string.Empty;

        [Required]
        [StringLength(20000, MinimumLength = 1)]
        public string BodyTemplate { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        public List<TemplateFieldDto> Fields { get; set; } = [];

        public List<int> ReminderOffsets { get; set; } = [10];

        public bool IsSystemTemplate { get; set; }
    }
}