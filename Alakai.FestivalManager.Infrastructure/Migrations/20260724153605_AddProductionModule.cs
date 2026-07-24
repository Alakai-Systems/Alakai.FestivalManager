#nullable disable

namespace Alakai.FestivalManager.Infrastructure.Migrations;

/// <inheritdoc />
public partial class AddProductionModule : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "ProductionAccommodationBuildings",
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
                table.PrimaryKey("PK_ProductionAccommodationBuildings", x => x.Id);
                table.ForeignKey(
                    name: "FK_ProductionAccommodationBuildings_Editions_EditionId",
                    column: x => x.EditionId,
                    principalTable: "Editions",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "ProductionPeople",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                EditionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Category = table.Column<int>(type: "int", nullable: false),
                RoleTitle = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                Phone = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                DocumentType = table.Column<int>(type: "int", nullable: false),
                DocumentNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                Nationality = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                IsActive = table.Column<bool>(type: "bit", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ProductionPeople", x => x.Id);
                table.ForeignKey(
                    name: "FK_ProductionPeople_Editions_EditionId",
                    column: x => x.EditionId,
                    principalTable: "Editions",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "ProductionSuppliers",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                EditionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                ServiceType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                ContactName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                Phone = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                IsActive = table.Column<bool>(type: "bit", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ProductionSuppliers", x => x.Id);
                table.ForeignKey(
                    name: "FK_ProductionSuppliers_Editions_EditionId",
                    column: x => x.EditionId,
                    principalTable: "Editions",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "RunnerItineraries",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                EditionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                Location = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                Direction = table.Column<int>(type: "int", nullable: false),
                RunnerName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_RunnerItineraries", x => x.Id);
                table.ForeignKey(
                    name: "FK_RunnerItineraries_Editions_EditionId",
                    column: x => x.EditionId,
                    principalTable: "Editions",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "ProductionAccommodationZones",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ProductionAccommodationBuildingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                SortOrder = table.Column<int>(type: "int", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ProductionAccommodationZones", x => x.Id);
                table.ForeignKey(
                    name: "FK_ProductionAccommodationZones_ProductionAccommodationBuildings_ProductionAccommodationBuildingId",
                    column: x => x.ProductionAccommodationBuildingId,
                    principalTable: "ProductionAccommodationBuildings",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "ProductionAccommodationReservations",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                EditionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ProductionAccommodationBuildingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ResponsibleProductionPersonId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ProductionAccommodationReservations", x => x.Id);
                table.ForeignKey(
                    name: "FK_ProductionAccommodationReservations_Editions_EditionId",
                    column: x => x.EditionId,
                    principalTable: "Editions",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_ProductionAccommodationReservations_ProductionAccommodationBuildings_ProductionAccommodationBuildingId",
                    column: x => x.ProductionAccommodationBuildingId,
                    principalTable: "ProductionAccommodationBuildings",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_ProductionAccommodationReservations_ProductionPeople_ResponsibleProductionPersonId",
                    column: x => x.ResponsibleProductionPersonId,
                    principalTable: "ProductionPeople",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.SetNull);
            });

        migrationBuilder.CreateTable(
            name: "ProductionTrips",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                EditionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ProductionPersonId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Type = table.Column<int>(type: "int", nullable: false),
                TripNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                TerminalOrStation = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                Direction = table.Column<int>(type: "int", nullable: false),
                RunnerItineraryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ProductionTrips", x => x.Id);
                table.ForeignKey(
                    name: "FK_ProductionTrips_Editions_EditionId",
                    column: x => x.EditionId,
                    principalTable: "Editions",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_ProductionTrips_ProductionPeople_ProductionPersonId",
                    column: x => x.ProductionPersonId,
                    principalTable: "ProductionPeople",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_ProductionTrips_RunnerItineraries_RunnerItineraryId",
                    column: x => x.RunnerItineraryId,
                    principalTable: "RunnerItineraries",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.SetNull);
            });

        migrationBuilder.CreateTable(
            name: "ProductionAccommodations",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ProductionAccommodationZoneId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                Capacity = table.Column<int>(type: "int", nullable: false),
                SortOrder = table.Column<int>(type: "int", nullable: false),
                IsActive = table.Column<bool>(type: "bit", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ProductionAccommodations", x => x.Id);
                table.ForeignKey(
                    name: "FK_ProductionAccommodations_ProductionAccommodationZones_ProductionAccommodationZoneId",
                    column: x => x.ProductionAccommodationZoneId,
                    principalTable: "ProductionAccommodationZones",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "ProductionAccommodationReservationOccupants",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ProductionAccommodationReservationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ProductionPersonId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                IsResponsible = table.Column<bool>(type: "bit", nullable: false),
                ProductionAccommodationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ProductionAccommodationReservationOccupants", x => x.Id);
                table.ForeignKey(
                    name: "FK_ProductionAccommodationReservationOccupants_ProductionAccommodationReservations_ProductionAccommodationReservationId",
                    column: x => x.ProductionAccommodationReservationId,
                    principalTable: "ProductionAccommodationReservations",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_ProductionAccommodationReservationOccupants_ProductionAccommodations_ProductionAccommodationId",
                    column: x => x.ProductionAccommodationId,
                    principalTable: "ProductionAccommodations",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_ProductionAccommodationReservationOccupants_ProductionPeople_ProductionPersonId",
                    column: x => x.ProductionPersonId,
                    principalTable: "ProductionPeople",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "IX_ProductionAccommodationBuildings_EditionId",
            table: "ProductionAccommodationBuildings",
            column: "EditionId");

        migrationBuilder.CreateIndex(
            name: "IX_ProductionAccommodationReservationOccupants_ProductionAccommodationId",
            table: "ProductionAccommodationReservationOccupants",
            column: "ProductionAccommodationId");

        migrationBuilder.CreateIndex(
            name: "IX_ProductionAccommodationReservationOccupants_ProductionAccommodationReservationId",
            table: "ProductionAccommodationReservationOccupants",
            column: "ProductionAccommodationReservationId");

        migrationBuilder.CreateIndex(
            name: "IX_ProductionAccommodationReservationOccupants_ProductionPersonId",
            table: "ProductionAccommodationReservationOccupants",
            column: "ProductionPersonId");

        migrationBuilder.CreateIndex(
            name: "IX_ProductionAccommodationReservations_EditionId",
            table: "ProductionAccommodationReservations",
            column: "EditionId");

        migrationBuilder.CreateIndex(
            name: "IX_ProductionAccommodationReservations_ProductionAccommodationBuildingId",
            table: "ProductionAccommodationReservations",
            column: "ProductionAccommodationBuildingId");

        migrationBuilder.CreateIndex(
            name: "IX_ProductionAccommodationReservations_ResponsibleProductionPersonId",
            table: "ProductionAccommodationReservations",
            column: "ResponsibleProductionPersonId");

        migrationBuilder.CreateIndex(
            name: "IX_ProductionAccommodations_ProductionAccommodationZoneId",
            table: "ProductionAccommodations",
            column: "ProductionAccommodationZoneId");

        migrationBuilder.CreateIndex(
            name: "IX_ProductionAccommodationZones_ProductionAccommodationBuildingId",
            table: "ProductionAccommodationZones",
            column: "ProductionAccommodationBuildingId");

        migrationBuilder.CreateIndex(
            name: "IX_ProductionPeople_EditionId_DocumentNumber",
            table: "ProductionPeople",
            columns: new[] { "EditionId", "DocumentNumber" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_ProductionSuppliers_EditionId",
            table: "ProductionSuppliers",
            column: "EditionId");

        migrationBuilder.CreateIndex(
            name: "IX_ProductionTrips_EditionId",
            table: "ProductionTrips",
            column: "EditionId");

        migrationBuilder.CreateIndex(
            name: "IX_ProductionTrips_ProductionPersonId",
            table: "ProductionTrips",
            column: "ProductionPersonId");

        migrationBuilder.CreateIndex(
            name: "IX_ProductionTrips_RunnerItineraryId",
            table: "ProductionTrips",
            column: "RunnerItineraryId");

        migrationBuilder.CreateIndex(
            name: "IX_RunnerItineraries_EditionId",
            table: "RunnerItineraries",
            column: "EditionId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "ProductionAccommodationReservationOccupants");

        migrationBuilder.DropTable(
            name: "ProductionSuppliers");

        migrationBuilder.DropTable(
            name: "ProductionTrips");

        migrationBuilder.DropTable(
            name: "ProductionAccommodationReservations");

        migrationBuilder.DropTable(
            name: "ProductionAccommodations");

        migrationBuilder.DropTable(
            name: "RunnerItineraries");

        migrationBuilder.DropTable(
            name: "ProductionPeople");

        migrationBuilder.DropTable(
            name: "ProductionAccommodationZones");

        migrationBuilder.DropTable(
            name: "ProductionAccommodationBuildings");
    }
}
