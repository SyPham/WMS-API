using Microsoft.EntityFrameworkCore.Migrations;

namespace Data.Migrations
{
    public partial class update601 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "Tasks",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TaskCode",
                table: "Histories",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Code",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "TaskCode",
                table: "Histories");
        }
    }
}
