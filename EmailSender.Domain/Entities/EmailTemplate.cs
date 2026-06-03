using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmailSender.Domain.Entities
{
    public class EmailTemplate
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public string TemplateName { get; set; } = string.Empty;

        public string SubjectTemplate { get; set; } = string.Empty;

        public string BodyTemplate { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string FieldsJson { get; set; } = "[]";
        public string ReminderOffsetsJson { get; set; } = "[10]";

        public DateTimeOffset CreatedAt { get; set; }

        public AppUser User { get; set; } = null!;
        public bool IsSystemTemplate { get; set; }

        // Scheduling configuration for template-based scheduled emails
        public int SchedulingOffsetDays { get; set; } = 0;
        public string SchedulingTargetField { get; set; } = string.Empty;
    }
}
