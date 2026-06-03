using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmailSender.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSchedulingConfigToEmailTemplates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SchedulingOffsetDays",
                table: "EmailTemplates",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "SchedulingTargetField",
                table: "EmailTemplates",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SchedulingOffsetDays",
                table: "EmailTemplates");

            migrationBuilder.DropColumn(
                name: "SchedulingTargetField",
                table: "EmailTemplates");
        }
    }
}
