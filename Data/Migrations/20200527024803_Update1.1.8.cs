using Microsoft.EntityFrameworkCore.Migrations;

namespace Data.Migrations
{
    public partial class Update118 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Comments_TaskID",
                table: "Comments");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_TaskID",
                table: "Comments",
                column: "TaskID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Comments_TaskID",
                table: "Comments");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_TaskID",
                table: "Comments",
                column: "TaskID",
                unique: true);
        }
    }
}
