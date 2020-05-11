using Microsoft.EntityFrameworkCore.Migrations;

namespace Data.Migrations
{
    public partial class version106 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserID",
                table: "Projects",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Projects_UserID",
                table: "Projects",
                column: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_Users_UserID",
                table: "Projects",
                column: "UserID",
                principalTable: "Users",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Projects_Users_UserID",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Projects_UserID",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "UserID",
                table: "Projects");
        }
    }
}
