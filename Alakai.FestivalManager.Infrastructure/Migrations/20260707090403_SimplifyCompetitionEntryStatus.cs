using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Alakai.FestivalManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SimplifyCompetitionEntryStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Data-only migration: remap CompetitionEntryStatus values after removing
            // the "Registered" member (old: Registered=1, Confirmed=2, WaitingPartner=3,
            // Cancelled=4 -> new: Confirmed=1, WaitingPartner=2, Cancelled=3).
            // Ascending order is safe: each UPDATE frees the value the next one writes.
            migrationBuilder.Sql("UPDATE CompetitionEntries SET Status = 1 WHERE Status = 2;");
            migrationBuilder.Sql("UPDATE CompetitionEntries SET Status = 2 WHERE Status = 3;");
            migrationBuilder.Sql("UPDATE CompetitionEntries SET Status = 3 WHERE Status = 4;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Reverse remap (descending order). Old Registered and Confirmed were merged
            // into Confirmed, so everything with Status=1 is restored as Confirmed=2.
            migrationBuilder.Sql("UPDATE CompetitionEntries SET Status = 4 WHERE Status = 3;");
            migrationBuilder.Sql("UPDATE CompetitionEntries SET Status = 3 WHERE Status = 2;");
            migrationBuilder.Sql("UPDATE CompetitionEntries SET Status = 2 WHERE Status = 1;");
        }
    }
}
