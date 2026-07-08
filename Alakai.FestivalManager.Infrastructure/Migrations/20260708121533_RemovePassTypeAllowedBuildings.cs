#nullable disable

namespace Alakai.FestivalManager.Infrastructure.Migrations;

/// <inheritdoc />
public partial class RemovePassTypeAllowedBuildings : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_AccommodationBuildingPassTypes_PassTypes_PassTypeId1",
            table: "AccommodationBuildingPassTypes");

        migrationBuilder.DropIndex(
            name: "IX_AccommodationBuildingPassTypes_PassTypeId1",
            table: "AccommodationBuildingPassTypes");

        migrationBuilder.DropColumn(
            name: "PassTypeId1",
            table: "AccommodationBuildingPassTypes");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<Guid>(
            name: "PassTypeId1",
            table: "AccommodationBuildingPassTypes",
            type: "uniqueidentifier",
            nullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_AccommodationBuildingPassTypes_PassTypeId1",
            table: "AccommodationBuildingPassTypes",
            column: "PassTypeId1");

        migrationBuilder.AddForeignKey(
            name: "FK_AccommodationBuildingPassTypes_PassTypes_PassTypeId1",
            table: "AccommodationBuildingPassTypes",
            column: "PassTypeId1",
            principalTable: "PassTypes",
            principalColumn: "Id");
    }
}
