using Microsoft.EntityFrameworkCore.Migrations;

namespace Data.Migrations
{
    public partial class version107 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.CreateIndex(
                name: "IX_Projects_CreatedBy",
                table: "Projects",
                column: "CreatedBy");

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_Users_CreatedBy",
                table: "Projects",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "ID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Projects_Users_CreatedBy",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Projects_CreatedBy",
                table: "Projects");

            migrationBuilder.AddColumn<int>(
                name: "UserID",
                table: "Projects",
                type: "int",
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
    }
}
