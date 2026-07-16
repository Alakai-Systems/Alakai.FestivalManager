using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Alakai.FestivalManager.Infrastructure.Migrations;

/// <inheritdoc />
public partial class AddFestivalCredentials : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "FaviconUrl",
            table: "Festivals",
            type: "nvarchar(500)",
            maxLength: 500,
            nullable: true);

        migrationBuilder.CreateTable(
            name: "FestivalCredentials",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                FestivalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                RedsysMerchantCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                RedsysTerminal = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                RedsysSecretKey = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                RedsysMerchantName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                EmailHost = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                EmailPort = table.Column<int>(type: "int", nullable: false),
                EmailUsername = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                EmailPassword = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                EmailFromEmail = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                EmailFromName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_FestivalCredentials", x => x.Id);
                table.ForeignKey(
                    name: "FK_FestivalCredentials_Festivals_FestivalId",
                    column: x => x.FestivalId,
                    principalTable: "Festivals",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_FestivalCredentials_FestivalId",
            table: "FestivalCredentials",
            column: "FestivalId",
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "FestivalCredentials");

        migrationBuilder.DropColumn(
            name: "FaviconUrl",
            table: "Festivals");
    }
}
