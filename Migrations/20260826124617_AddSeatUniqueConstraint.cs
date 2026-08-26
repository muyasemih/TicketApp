using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TicketApp.Migrations
{
    /// <inheritdoc />
    public partial class AddSeatUniqueConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Seats_VenueBlockId",
                table: "Seats");

            migrationBuilder.CreateIndex(
                name: "IX_Seats_VenueBlockId_RowNumber_SeatNumber",
                table: "Seats",
                columns: new[] { "VenueBlockId", "RowNumber", "SeatNumber" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Seats_VenueBlockId_RowNumber_SeatNumber",
                table: "Seats");

            migrationBuilder.CreateIndex(
                name: "IX_Seats_VenueBlockId",
                table: "Seats",
                column: "VenueBlockId");
        }
    }
}
