using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Alakai.FestivalManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCompetitionCapacityToCompetitionEntries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CompetitionEntries_CompetitionId_RegistrationId",
                table: "CompetitionEntries");

            migrationBuilder.AddColumn<Guid>(
                name: "CompetitionCapacityId",
                table: "CompetitionEntries",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_CompetitionEntries_CompetitionCapacityId",
                table: "CompetitionEntries",
                column: "CompetitionCapacityId");

            migrationBuilder.CreateIndex(
                name: "IX_CompetitionEntries_CompetitionId_RegistrationId_CompetitionCapacityId",
                table: "CompetitionEntries",
                columns: new[] { "CompetitionId", "RegistrationId", "CompetitionCapacityId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_CompetitionEntries_CompetitionCapacities_CompetitionCapacityId",
                table: "CompetitionEntries",
                column: "CompetitionCapacityId",
                principalTable: "CompetitionCapacities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CompetitionEntries_CompetitionCapacities_CompetitionCapacityId",
                table: "CompetitionEntries");

            migrationBuilder.DropIndex(
                name: "IX_CompetitionEntries_CompetitionCapacityId",
                table: "CompetitionEntries");

            migrationBuilder.DropIndex(
                name: "IX_CompetitionEntries_CompetitionId_RegistrationId_CompetitionCapacityId",
                table: "CompetitionEntries");

            migrationBuilder.DropColumn(
                name: "CompetitionCapacityId",
                table: "CompetitionEntries");

            migrationBuilder.CreateIndex(
                name: "IX_CompetitionEntries_CompetitionId_RegistrationId",
                table: "CompetitionEntries",
                columns: new[] { "CompetitionId", "RegistrationId" },
                unique: true);
        }
    }
}
