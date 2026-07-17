using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Alakai.FestivalManager.Infrastructure.Migrations;

/// <inheritdoc />
public partial class AddImageWidthsToEmailLayout : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(
            name: "FooterImageWidth",
            table: "EmailLayouts",
            type: "int",
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "HeaderImageWidth",
            table: "EmailLayouts",
            type: "int",
            nullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "FooterImageWidth",
            table: "EmailLayouts");

        migrationBuilder.DropColumn(
            name: "HeaderImageWidth",
            table: "EmailLayouts");
    }
}
