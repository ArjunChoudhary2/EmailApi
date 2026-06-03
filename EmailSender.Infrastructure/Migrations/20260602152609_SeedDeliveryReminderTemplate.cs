using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmailSender.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedDeliveryReminderTemplate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create a system user for templates if it doesn't exist
            migrationBuilder.Sql(
                @"INSERT INTO ""Users"" (""Id"", ""GoogleSubject"", ""Email"", ""DisplayName"", ""CreatedAt"", ""UpdatedAt"")
                  VALUES ('00000000-0000-0000-0000-000000000001'::uuid, 'system-user', 'system@example.com', 'System', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP)
                  ON CONFLICT DO NOTHING"
            );

            // Seed DeliveryReminder template with scheduling configuration
            migrationBuilder.Sql(
                @"INSERT INTO ""EmailTemplates"" (""Id"", ""UserId"", ""TemplateName"", ""SubjectTemplate"", ""BodyTemplate"", ""Description"", ""FieldsJson"", ""ReminderOffsetsJson"", ""CreatedAt"", ""IsSystemTemplate"", ""SchedulingOffsetDays"", ""SchedulingTargetField"")
                  VALUES ('00000000-0000-0000-0000-000000000002'::uuid, '00000000-0000-0000-0000-000000000001'::uuid, 'DeliveryReminder', 'Your delivery is scheduled for {{deliveryDate}}', 'Dear customer,\n\nThis is a reminder that your order will be delivered on {{deliveryDate}}.\n\nBest regards,\nOur Team', 'System template for delivery reminders', '[""deliveryDate""]'::jsonb, '[10]'::jsonb, CURRENT_TIMESTAMP, true, 3, 'deliveryDate')
                  ON CONFLICT DO NOTHING"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove seeded DeliveryReminder template
            migrationBuilder.Sql(
                @"DELETE FROM ""EmailTemplates"" WHERE ""TemplateName"" = 'DeliveryReminder' AND ""IsSystemTemplate"" = true"
            );
            
            // Remove system user (if it doesn't have other templates)
            migrationBuilder.Sql(
                @"DELETE FROM ""Users"" WHERE ""Id"" = '00000000-0000-0000-0000-000000000001'::uuid AND NOT EXISTS (
                    SELECT 1 FROM ""EmailTemplates"" WHERE ""UserId"" = '00000000-0000-0000-0000-000000000001'::uuid
                )"
            );
        }
    }
}
