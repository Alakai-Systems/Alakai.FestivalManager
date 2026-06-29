using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Alakai.FestivalManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailLayout : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EmailLayouts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HeaderHtml = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HeaderText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FooterHtml = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FooterText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailLayouts", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmailLayouts");
        }
    }
}
