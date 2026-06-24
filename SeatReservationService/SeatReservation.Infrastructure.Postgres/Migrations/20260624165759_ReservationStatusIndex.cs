using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SeatReservation.Infrastructure.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class ReservationStatusIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_events_status",
                table: "events");

            migrationBuilder.CreateIndex(
                name: "IX_reservations_event_id_status",
                table: "reservations",
                columns: new[] { "event_id", "status" },
                filter: "status IN ('Confirmed', 'Pending')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_reservations_event_id_status",
                table: "reservations");

            migrationBuilder.CreateIndex(
                name: "IX_events_status",
                table: "events",
                column: "status",
                filter: "status IN ('Confirmed', 'Pending')");
        }
    }
}
