using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Alakai.FestivalManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStripeIntegration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EnabledPaymentPlatforms",
                table: "Festivals",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "StripePublishableKey",
                table: "FestivalCredentials",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StripeSecretKey",
                table: "FestivalCredentials",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StripeWebhookSecret",
                table: "FestivalCredentials",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EnabledPaymentPlatforms",
                table: "Festivals");

            migrationBuilder.DropColumn(
                name: "StripePublishableKey",
                table: "FestivalCredentials");

            migrationBuilder.DropColumn(
                name: "StripeSecretKey",
                table: "FestivalCredentials");

            migrationBuilder.DropColumn(
                name: "StripeWebhookSecret",
                table: "FestivalCredentials");
        }
    }
}
