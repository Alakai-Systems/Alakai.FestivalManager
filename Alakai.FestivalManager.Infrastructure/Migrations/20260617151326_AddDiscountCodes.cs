using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Alakai.FestivalManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDiscountCodes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DiscountCode",
                table: "Registrations",
                newName: "DiscountCodeValue");

            migrationBuilder.AddColumn<Guid>(
                name: "DiscountCodeId",
                table: "Registrations",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DiscountCodeId1",
                table: "Registrations",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DiscountStatus",
                table: "Registrations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "DiscountCodes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EditionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    DiscountType = table.Column<int>(type: "int", nullable: false),
                    DiscountValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ActivationType = table.Column<int>(type: "int", nullable: false),
                    ActivationThreshold = table.Column<int>(type: "int", nullable: true),
                    MaxUses = table.Column<int>(type: "int", nullable: true),
                    CurrentUses = table.Column<int>(type: "int", nullable: false),
                    StartsAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndsAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiscountCodes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DiscountCodes_Editions_EditionId",
                        column: x => x.EditionId,
                        principalTable: "Editions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Registrations_DiscountCodeId",
                table: "Registrations",
                column: "DiscountCodeId");

            migrationBuilder.CreateIndex(
                name: "IX_Registrations_DiscountCodeId1",
                table: "Registrations",
                column: "DiscountCodeId1");

            migrationBuilder.CreateIndex(
                name: "IX_DiscountCodes_EditionId",
                table: "DiscountCodes",
                column: "EditionId");

            migrationBuilder.CreateIndex(
                name: "IX_DiscountCodes_EditionId_Code",
                table: "DiscountCodes",
                columns: new[] { "EditionId", "Code" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Registrations_DiscountCodes_DiscountCodeId",
                table: "Registrations",
                column: "DiscountCodeId",
                principalTable: "DiscountCodes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Registrations_DiscountCodes_DiscountCodeId1",
                table: "Registrations",
                column: "DiscountCodeId1",
                principalTable: "DiscountCodes",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Registrations_DiscountCodes_DiscountCodeId",
                table: "Registrations");

            migrationBuilder.DropForeignKey(
                name: "FK_Registrations_DiscountCodes_DiscountCodeId1",
                table: "Registrations");

            migrationBuilder.DropTable(
                name: "DiscountCodes");

            migrationBuilder.DropIndex(
                name: "IX_Registrations_DiscountCodeId",
                table: "Registrations");

            migrationBuilder.DropIndex(
                name: "IX_Registrations_DiscountCodeId1",
                table: "Registrations");

            migrationBuilder.DropColumn(
                name: "DiscountCodeId",
                table: "Registrations");

            migrationBuilder.DropColumn(
                name: "DiscountCodeId1",
                table: "Registrations");

            migrationBuilder.DropColumn(
                name: "DiscountStatus",
                table: "Registrations");

            migrationBuilder.RenameColumn(
                name: "DiscountCodeValue",
                table: "Registrations",
                newName: "DiscountCode");
        }
    }
}
