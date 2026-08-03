#nullable disable

namespace Alakai.FestivalManager.Infrastructure.Migrations;

/// <inheritdoc />
public partial class AddTicketFieldsToRegistration : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<DateTime>(
            name: "CheckedInAt",
            table: "Registrations",
            type: "datetime2",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "TicketPdfUrl",
            table: "Registrations",
            type: "nvarchar(500)",
            maxLength: 500,
            nullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "CheckedInAt",
            table: "Registrations");

        migrationBuilder.DropColumn(
            name: "TicketPdfUrl",
            table: "Registrations");
    }
}
