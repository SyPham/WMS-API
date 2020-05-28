using Microsoft.EntityFrameworkCore.Migrations;

namespace Data.Migrations
{
    public partial class Update117 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Pin",
                table: "CommentDetails",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Pin",
                table: "CommentDetails");
        }
    }
}
