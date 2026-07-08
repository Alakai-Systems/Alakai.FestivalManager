using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Alakai.FestivalManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentPlanToRegistration2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "AmountPaid",
                table: "Registrations",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ManagementFee",
                table: "Registrations",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "PaymentDueAt",
                table: "Registrations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PaymentPlan",
                table: "Registrations",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AmountPaid",
                table: "Registrations");

            migrationBuilder.DropColumn(
                name: "ManagementFee",
                table: "Registrations");

            migrationBuilder.DropColumn(
                name: "PaymentDueAt",
                table: "Registrations");

            migrationBuilder.DropColumn(
                name: "PaymentPlan",
                table: "Registrations");
        }
    }
}
