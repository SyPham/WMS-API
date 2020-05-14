using Microsoft.EntityFrameworkCore.Migrations;

namespace Data.Migrations
{
    public partial class version109 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DueDateDaily",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "DueDateMonthly",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "DueDateWeekly",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "SpecificDate",
                table: "Tasks");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DueDateDaily",
                table: "Tasks",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DueDateMonthly",
                table: "Tasks",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DueDateWeekly",
                table: "Tasks",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SpecificDate",
                table: "Tasks",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
