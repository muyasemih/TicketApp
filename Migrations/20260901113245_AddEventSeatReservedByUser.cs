using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TicketApp.Migrations
{
    /// <inheritdoc />
    public partial class AddEventSeatReservedByUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ReservedByUserId",
                table: "EventSeats",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_EventSeats_ReservedByUserId",
                table: "EventSeats",
                column: "ReservedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_EventSeats_Users_ReservedByUserId",
                table: "EventSeats",
                column: "ReservedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EventSeats_Users_ReservedByUserId",
                table: "EventSeats");

            migrationBuilder.DropIndex(
                name: "IX_EventSeats_ReservedByUserId",
                table: "EventSeats");

            migrationBuilder.DropColumn(
                name: "ReservedByUserId",
                table: "EventSeats");
        }
    }
}
