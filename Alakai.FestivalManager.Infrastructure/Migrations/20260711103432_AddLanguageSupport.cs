using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Alakai.FestivalManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLanguageSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EmailTemplates_EditionId_TemplateKey",
                table: "EmailTemplates");

            migrationBuilder.AddColumn<string>(
                name: "Language",
                table: "Registrations",
                type: "nvarchar(5)",
                maxLength: 5,
                nullable: false,
                defaultValue: "en");

            migrationBuilder.AddColumn<string>(
                name: "Language",
                table: "EmailTemplates",
                type: "nvarchar(5)",
                maxLength: 5,
                nullable: false,
                defaultValue: "en");

            migrationBuilder.CreateIndex(
                name: "IX_EmailTemplates_EditionId_TemplateKey_Language",
                table: "EmailTemplates",
                columns: new[] { "EditionId", "TemplateKey", "Language" },
                unique: true,
                filter: "[EditionId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EmailTemplates_EditionId_TemplateKey_Language",
                table: "EmailTemplates");

            migrationBuilder.DropColumn(
                name: "Language",
                table: "Registrations");

            migrationBuilder.DropColumn(
                name: "Language",
                table: "EmailTemplates");

            migrationBuilder.CreateIndex(
                name: "IX_EmailTemplates_EditionId_TemplateKey",
                table: "EmailTemplates",
                columns: new[] { "EditionId", "TemplateKey" },
                unique: true,
                filter: "[EditionId] IS NOT NULL");
        }
    }
}
