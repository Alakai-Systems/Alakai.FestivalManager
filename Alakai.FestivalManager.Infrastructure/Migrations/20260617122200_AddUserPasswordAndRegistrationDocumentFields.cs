

#nullable disable

namespace Alakai.FestivalManager.Infrastructure.Migrations;

/// <inheritdoc />
public partial class AddUserPasswordAndRegistrationDocumentFields : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<DateTime>(
            name: "LastLoginAt",
            table: "Users",
            type: "datetime2",
            nullable: true);

        migrationBuilder.AddColumn<bool>(
            name: "MustChangePassword",
            table: "Users",
            type: "bit",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<string>(
            name: "PasswordHash",
            table: "Users",
            type: "nvarchar(500)",
            maxLength: 500,
            nullable: false,
            defaultValue: "");

        migrationBuilder.AddColumn<string>(
            name: "PasswordSalt",
            table: "Users",
            type: "nvarchar(500)",
            maxLength: 500,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "DocumentCountry",
            table: "Registrations",
            type: "nvarchar(100)",
            maxLength: 100,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "DocumentNumber",
            table: "Registrations",
            type: "nvarchar(100)",
            maxLength: 100,
            nullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "LastLoginAt",
            table: "Users");

        migrationBuilder.DropColumn(
            name: "MustChangePassword",
            table: "Users");

        migrationBuilder.DropColumn(
            name: "PasswordHash",
            table: "Users");

        migrationBuilder.DropColumn(
            name: "PasswordSalt",
            table: "Users");

        migrationBuilder.DropColumn(
            name: "DocumentCountry",
            table: "Registrations");

        migrationBuilder.DropColumn(
            name: "DocumentNumber",
            table: "Registrations");
    }
}
