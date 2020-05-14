using Microsoft.EntityFrameworkCore.Migrations;

namespace Data.Migrations
{
    public partial class version108 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DueDateTime",
                table: "Tasks",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DueDateTime",
                table: "Tasks");
        }
    }
}
