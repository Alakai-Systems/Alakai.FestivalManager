using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Alakai.FestivalManager.Infrastructure.Migrations;

/// <inheritdoc />
public partial class AddProductionPersonManagerEmail : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "ManagerEmail",
            table: "ProductionPeople",
            type: "nvarchar(max)",
            nullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "ManagerEmail",
            table: "ProductionPeople");
    }
}
