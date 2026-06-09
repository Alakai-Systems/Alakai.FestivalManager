#nullable disable

namespace Alakai.FestivalManager.Infrastructure.Migrations;

/// <inheritdoc />
public partial class InitialCreate : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Festivals",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                Slug = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                Website = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                LogoUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                IsActive = table.Column<bool>(type: "bit", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Festivals", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Editions",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                FestivalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                Year = table.Column<int>(type: "int", nullable: false),
                StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                RegistrationOpenDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                RegistrationCloseDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                IsActive = table.Column<bool>(type: "bit", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Editions", x => x.Id);
                table.ForeignKey(
                    name: "FK_Editions_Festivals_FestivalId",
                    column: x => x.FestivalId,
                    principalTable: "Festivals",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "PassTypes",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                EditionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                SortOrder = table.Column<int>(type: "int", nullable: false),
                IsActive = table.Column<bool>(type: "bit", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PassTypes", x => x.Id);
                table.ForeignKey(
                    name: "FK_PassTypes_Editions_EditionId",
                    column: x => x.EditionId,
                    principalTable: "Editions",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "Levels",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                PassTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                EarlyBirdPrice = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                GroupPrice = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                RegularPrice = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                LeaderCapacity = table.Column<int>(type: "int", nullable: true),
                FollowerCapacity = table.Column<int>(type: "int", nullable: true),
                IndividualCapacity = table.Column<int>(type: "int", nullable: true),
                MaxLeaderDifference = table.Column<int>(type: "int", nullable: true),
                MaxFollowerDifference = table.Column<int>(type: "int", nullable: true),
                SortOrder = table.Column<int>(type: "int", nullable: false),
                IsActive = table.Column<bool>(type: "bit", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Levels", x => x.Id);
                table.ForeignKey(
                    name: "FK_Levels_PassTypes_PassTypeId",
                    column: x => x.PassTypeId,
                    principalTable: "PassTypes",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Editions_FestivalId_Year",
            table: "Editions",
            columns: new[] { "FestivalId", "Year" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Festivals_Slug",
            table: "Festivals",
            column: "Slug",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Levels_PassTypeId_Name",
            table: "Levels",
            columns: new[] { "PassTypeId", "Name" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_PassTypes_EditionId_Name",
            table: "PassTypes",
            columns: new[] { "EditionId", "Name" },
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Levels");

        migrationBuilder.DropTable(
            name: "PassTypes");

        migrationBuilder.DropTable(
            name: "Editions");

        migrationBuilder.DropTable(
            name: "Festivals");
    }
}
