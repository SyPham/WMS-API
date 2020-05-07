using Microsoft.EntityFrameworkCore.Migrations;

namespace Data.Migrations
{
    public partial class version101 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tags_UserID",
                table: "Tags");

            migrationBuilder.DropIndex(
                name: "IX_Deputies_UserID",
                table: "Deputies");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_UserID",
                table: "Tags",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_Deputies_UserID",
                table: "Deputies",
                column: "UserID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tags_UserID",
                table: "Tags");

            migrationBuilder.DropIndex(
                name: "IX_Deputies_UserID",
                table: "Deputies");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_UserID",
                table: "Tags",
                column: "UserID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Deputies_UserID",
                table: "Deputies",
                column: "UserID",
                unique: true);
        }
    }
}
