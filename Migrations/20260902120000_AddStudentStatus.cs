using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TicketApp.Migrations
{
    public partial class AddStudentStatus : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsStudent",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsStudent",
                table: "Users");
        }
    }
}
