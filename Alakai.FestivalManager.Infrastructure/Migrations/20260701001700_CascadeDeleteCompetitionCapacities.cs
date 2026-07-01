using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Alakai.FestivalManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CascadeDeleteCompetitionCapacities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CompetitionCapacities_Competitions_CompetitionId",
                table: "CompetitionCapacities");

            migrationBuilder.DropForeignKey(
                name: "FK_CompetitionLevels_Competitions_CompetitionId",
                table: "CompetitionLevels");

            migrationBuilder.AddForeignKey(
                name: "FK_CompetitionCapacities_Competitions_CompetitionId",
                table: "CompetitionCapacities",
                column: "CompetitionId",
                principalTable: "Competitions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CompetitionLevels_Competitions_CompetitionId",
                table: "CompetitionLevels",
                column: "CompetitionId",
                principalTable: "Competitions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CompetitionCapacities_Competitions_CompetitionId",
                table: "CompetitionCapacities");

            migrationBuilder.DropForeignKey(
                name: "FK_CompetitionLevels_Competitions_CompetitionId",
                table: "CompetitionLevels");

            migrationBuilder.AddForeignKey(
                name: "FK_CompetitionCapacities_Competitions_CompetitionId",
                table: "CompetitionCapacities",
                column: "CompetitionId",
                principalTable: "Competitions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CompetitionLevels_Competitions_CompetitionId",
                table: "CompetitionLevels",
                column: "CompetitionId",
                principalTable: "Competitions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
