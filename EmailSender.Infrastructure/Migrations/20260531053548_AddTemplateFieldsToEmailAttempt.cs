using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmailSender.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTemplateFieldsToEmailAttempt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TemplateFieldValuesJson",
                table: "EmailAttempts",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TemplateId",
                table: "EmailAttempts",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TemplateFieldValuesJson",
                table: "EmailAttempts");

            migrationBuilder.DropColumn(
                name: "TemplateId",
                table: "EmailAttempts");
        }
    }
}
