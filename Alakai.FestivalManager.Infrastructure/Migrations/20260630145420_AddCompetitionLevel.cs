using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Alakai.FestivalManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCompetitionLevel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CompetitionLevelId",
                table: "CompetitionCapacities",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CompetitionLevels",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompetitionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompetitionLevels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompetitionLevels_Competitions_CompetitionId",
                        column: x => x.CompetitionId,
                        principalTable: "Competitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CompetitionCapacities_CompetitionLevelId",
                table: "CompetitionCapacities",
                column: "CompetitionLevelId");

            migrationBuilder.CreateIndex(
                name: "IX_CompetitionLevels_CompetitionId_Name",
                table: "CompetitionLevels",
                columns: new[] { "CompetitionId", "Name" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_CompetitionCapacities_CompetitionLevels_CompetitionLevelId",
                table: "CompetitionCapacities",
                column: "CompetitionLevelId",
                principalTable: "CompetitionLevels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CompetitionCapacities_CompetitionLevels_CompetitionLevelId",
                table: "CompetitionCapacities");

            migrationBuilder.DropTable(
                name: "CompetitionLevels");

            migrationBuilder.DropIndex(
                name: "IX_CompetitionCapacities_CompetitionLevelId",
                table: "CompetitionCapacities");

            migrationBuilder.DropColumn(
                name: "CompetitionLevelId",
                table: "CompetitionCapacities");
        }
    }
}
