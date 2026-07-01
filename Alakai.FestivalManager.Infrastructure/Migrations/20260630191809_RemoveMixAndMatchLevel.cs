using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Alakai.FestivalManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveMixAndMatchLevel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CompetitionCapacities_CompetitionId_MixAndMatchLevel_DanceRole",
                table: "CompetitionCapacities");

            migrationBuilder.DropColumn(
                name: "MixAndMatchLevel",
                table: "CompetitionCapacities");

            migrationBuilder.RenameColumn(
                name: "MixAndMatchLevel",
                table: "CompetitionEntries",
                newName: "TeamMemberCount");

            migrationBuilder.CreateIndex(
                name: "IX_CompetitionCapacities_CompetitionId_CompetitionLevelId_DanceRole",
                table: "CompetitionCapacities",
                columns: new[] { "CompetitionId", "CompetitionLevelId", "DanceRole" },
                unique: true,
                filter: "[CompetitionLevelId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CompetitionCapacities_CompetitionId_CompetitionLevelId_DanceRole",
                table: "CompetitionCapacities");

            migrationBuilder.RenameColumn(
                name: "TeamMemberCount",
                table: "CompetitionEntries",
                newName: "MixAndMatchLevel");

            migrationBuilder.AddColumn<int>(
                name: "MixAndMatchLevel",
                table: "CompetitionCapacities",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CompetitionCapacities_CompetitionId_MixAndMatchLevel_DanceRole",
                table: "CompetitionCapacities",
                columns: new[] { "CompetitionId", "MixAndMatchLevel", "DanceRole" },
                unique: true,
                filter: "[MixAndMatchLevel] IS NOT NULL");
        }
    }
}
