using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Alakai.FestivalManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMultiLevelFieldsToPassType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "AllLevelsDiscountPercent",
                table: "PassTypes",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "AllowsMultipleLevels",
                table: "PassTypes",
                type: "bit",
                nullable: false,
                defaultValue: false);

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AccommodationBuildingPassTypes_PassTypes_PassTypeId1",
                table: "AccommodationBuildingPassTypes");

            migrationBuilder.DropIndex(
                name: "IX_AccommodationBuildingPassTypes_PassTypeId1",
                table: "AccommodationBuildingPassTypes");

            migrationBuilder.DropColumn(
                name: "AllLevelsDiscountPercent",
                table: "PassTypes");

            migrationBuilder.DropColumn(
                name: "AllowsMultipleLevels",
                table: "PassTypes");

            migrationBuilder.DropColumn(
                name: "PassTypeId1",
                table: "AccommodationBuildingPassTypes");
        }
    }
}
