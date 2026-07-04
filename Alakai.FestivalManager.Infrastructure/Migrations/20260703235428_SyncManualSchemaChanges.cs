using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Alakai.FestivalManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SyncManualSchemaChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EnabledModules",
                table: "Festivals",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "AccommodationBuildings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EditionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    IsLocked = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccommodationBuildings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccommodationBuildings_Editions_EditionId",
                        column: x => x.EditionId,
                        principalTable: "Editions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Buses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EditionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Direction = table.Column<int>(type: "int", nullable: false),
                    DepartureTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PickupLocation = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    DestinationLocation = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Capacity = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Buses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Buses_Editions_EditionId",
                        column: x => x.EditionId,
                        principalTable: "Editions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AccommodationBuildingPassTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccommodationBuildingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PassTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccommodationBuildingPassTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccommodationBuildingPassTypes_AccommodationBuildings_AccommodationBuildingId",
                        column: x => x.AccommodationBuildingId,
                        principalTable: "AccommodationBuildings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AccommodationBuildingPassTypes_PassTypes_PassTypeId",
                        column: x => x.PassTypeId,
                        principalTable: "PassTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AccommodationReservations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EditionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccommodationBuildingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ResponsibleRegistrationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccommodationReservations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccommodationReservations_AccommodationBuildings_AccommodationBuildingId",
                        column: x => x.AccommodationBuildingId,
                        principalTable: "AccommodationBuildings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AccommodationReservations_Editions_EditionId",
                        column: x => x.EditionId,
                        principalTable: "Editions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AccommodationReservations_Registrations_ResponsibleRegistrationId",
                        column: x => x.ResponsibleRegistrationId,
                        principalTable: "Registrations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AccommodationZones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccommodationBuildingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccommodationZones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccommodationZones_AccommodationBuildings_AccommodationBuildingId",
                        column: x => x.AccommodationBuildingId,
                        principalTable: "AccommodationBuildings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BusPassTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BusId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PassTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BusPassTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BusPassTypes_Buses_BusId",
                        column: x => x.BusId,
                        principalTable: "Buses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BusPassTypes_PassTypes_PassTypeId",
                        column: x => x.PassTypeId,
                        principalTable: "PassTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BusReservations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BusId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RegistrationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BusReservations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BusReservations_Buses_BusId",
                        column: x => x.BusId,
                        principalTable: "Buses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BusReservations_Registrations_RegistrationId",
                        column: x => x.RegistrationId,
                        principalTable: "Registrations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Accommodations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccommodationZoneId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Capacity = table.Column<int>(type: "int", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accommodations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Accommodations_AccommodationZones_AccommodationZoneId",
                        column: x => x.AccommodationZoneId,
                        principalTable: "AccommodationZones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AccommodationReservationOccupants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccommodationReservationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    BirthDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DocumentExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsResponsible = table.Column<bool>(type: "bit", nullable: false),
                    RegistrationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AccommodationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccommodationReservationOccupants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccommodationReservationOccupants_AccommodationReservations_AccommodationReservationId",
                        column: x => x.AccommodationReservationId,
                        principalTable: "AccommodationReservations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AccommodationReservationOccupants_Accommodations_AccommodationId",
                        column: x => x.AccommodationId,
                        principalTable: "Accommodations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AccommodationReservationOccupants_Registrations_RegistrationId",
                        column: x => x.RegistrationId,
                        principalTable: "Registrations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccommodationBuildingPassTypes_AccommodationBuildingId_PassTypeId",
                table: "AccommodationBuildingPassTypes",
                columns: new[] { "AccommodationBuildingId", "PassTypeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AccommodationBuildingPassTypes_PassTypeId",
                table: "AccommodationBuildingPassTypes",
                column: "PassTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_AccommodationBuildings_EditionId",
                table: "AccommodationBuildings",
                column: "EditionId");

            migrationBuilder.CreateIndex(
                name: "IX_AccommodationReservationOccupants_AccommodationId",
                table: "AccommodationReservationOccupants",
                column: "AccommodationId");

            migrationBuilder.CreateIndex(
                name: "IX_AccommodationReservationOccupants_AccommodationReservationId",
                table: "AccommodationReservationOccupants",
                column: "AccommodationReservationId");

            migrationBuilder.CreateIndex(
                name: "IX_AccommodationReservationOccupants_RegistrationId",
                table: "AccommodationReservationOccupants",
                column: "RegistrationId");

            migrationBuilder.CreateIndex(
                name: "IX_AccommodationReservations_AccommodationBuildingId",
                table: "AccommodationReservations",
                column: "AccommodationBuildingId");

            migrationBuilder.CreateIndex(
                name: "IX_AccommodationReservations_EditionId",
                table: "AccommodationReservations",
                column: "EditionId");

            migrationBuilder.CreateIndex(
                name: "IX_AccommodationReservations_ResponsibleRegistrationId",
                table: "AccommodationReservations",
                column: "ResponsibleRegistrationId");

            migrationBuilder.CreateIndex(
                name: "IX_Accommodations_AccommodationZoneId",
                table: "Accommodations",
                column: "AccommodationZoneId");

            migrationBuilder.CreateIndex(
                name: "IX_AccommodationZones_AccommodationBuildingId",
                table: "AccommodationZones",
                column: "AccommodationBuildingId");

            migrationBuilder.CreateIndex(
                name: "IX_Buses_EditionId",
                table: "Buses",
                column: "EditionId");

            migrationBuilder.CreateIndex(
                name: "IX_BusPassTypes_BusId_PassTypeId",
                table: "BusPassTypes",
                columns: new[] { "BusId", "PassTypeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BusPassTypes_PassTypeId",
                table: "BusPassTypes",
                column: "PassTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_BusReservations_BusId",
                table: "BusReservations",
                column: "BusId");

            migrationBuilder.CreateIndex(
                name: "IX_BusReservations_RegistrationId",
                table: "BusReservations",
                column: "RegistrationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccommodationBuildingPassTypes");

            migrationBuilder.DropTable(
                name: "AccommodationReservationOccupants");

            migrationBuilder.DropTable(
                name: "BusPassTypes");

            migrationBuilder.DropTable(
                name: "BusReservations");

            migrationBuilder.DropTable(
                name: "AccommodationReservations");

            migrationBuilder.DropTable(
                name: "Accommodations");

            migrationBuilder.DropTable(
                name: "Buses");

            migrationBuilder.DropTable(
                name: "AccommodationZones");

            migrationBuilder.DropTable(
                name: "AccommodationBuildings");

            migrationBuilder.DropColumn(
                name: "EnabledModules",
                table: "Festivals");
        }
    }
}
