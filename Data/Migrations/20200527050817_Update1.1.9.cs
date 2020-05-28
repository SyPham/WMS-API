using Microsoft.EntityFrameworkCore.Migrations;

namespace Data.Migrations
{
    public partial class Update119 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Pin",
                table: "CommentDetails");

            migrationBuilder.AddColumn<bool>(
                name: "Pin",
                table: "Comments",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Pin",
                table: "Comments");

            migrationBuilder.AddColumn<bool>(
                name: "Pin",
                table: "CommentDetails",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
