using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TicketApp.Migrations
{
    /// <inheritdoc />
    public partial class AddReservedUntil : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ReservedUntil",
                table: "EventSeats",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReservedUntil",
                table: "EventSeats");
        }
    }
}
