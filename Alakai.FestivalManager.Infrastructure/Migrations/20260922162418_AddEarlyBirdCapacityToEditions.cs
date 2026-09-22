using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Alakai.FestivalManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEarlyBirdCapacityToEditions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsEarlyBirdPrice",
                table: "Registrations",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "EarlyBirdCapacity",
                table: "Editions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EarlyBirdUsedCount",
                table: "Editions",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsEarlyBirdPrice",
                table: "Registrations");

            migrationBuilder.DropColumn(
                name: "EarlyBirdCapacity",
                table: "Editions");

            migrationBuilder.DropColumn(
                name: "EarlyBirdUsedCount",
                table: "Editions");
        }
    }
}
