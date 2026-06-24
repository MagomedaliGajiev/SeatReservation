using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SeatReservation.Infrastructure.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class AddIndices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_seats_venue_id",
                table: "seats");

            migrationBuilder.CreateIndex(
                name: "IX_seats_venue_id_row_number_seat_number",
                table: "seats",
                columns: new[] { "venue_id", "row_number", "seat_number" });

            migrationBuilder.CreateIndex(
                name: "IX_events_event_date",
                table: "events",
                column: "event_date");

            migrationBuilder.CreateIndex(
                name: "IX_events_start_date",
                table: "events",
                column: "start_date");

            migrationBuilder.CreateIndex(
                name: "IX_events_status",
                table: "events",
                column: "status",
                filter: "status IN ('Confirmed', 'Pending')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_seats_venue_id_row_number_seat_number",
                table: "seats");

            migrationBuilder.DropIndex(
                name: "IX_events_event_date",
                table: "events");

            migrationBuilder.DropIndex(
                name: "IX_events_start_date",
                table: "events");

            migrationBuilder.DropIndex(
                name: "IX_events_status",
                table: "events");

            migrationBuilder.CreateIndex(
                name: "IX_seats_venue_id",
                table: "seats",
                column: "venue_id");
        }
    }
}
