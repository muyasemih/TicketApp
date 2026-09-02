using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TicketApp.Migrations
{
    /// <inheritdoc />
    public partial class AddEventBlockPrices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EventBlockPrices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventId = table.Column<int>(type: "int", nullable: false),
                    VenueBlockId = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventBlockPrices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventBlockPrices_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EventBlockPrices_VenueBlocks_VenueBlockId",
                        column: x => x.VenueBlockId,
                        principalTable: "VenueBlocks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EventBlockPrices_EventId_VenueBlockId",
                table: "EventBlockPrices",
                columns: new[] { "EventId", "VenueBlockId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EventBlockPrices_VenueBlockId",
                table: "EventBlockPrices",
                column: "VenueBlockId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EventBlockPrices");
        }
    }
}
