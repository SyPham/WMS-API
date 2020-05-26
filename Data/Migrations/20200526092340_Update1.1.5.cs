using Microsoft.EntityFrameworkCore.Migrations;

namespace Data.Migrations
{
    public partial class Update115 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Comments_TaskID",
                table: "Comments",
                column: "TaskID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CommentDetails_CommentID",
                table: "CommentDetails",
                column: "CommentID");

            migrationBuilder.AddForeignKey(
                name: "FK_CommentDetails_Comments_CommentID",
                table: "CommentDetails",
                column: "CommentID",
                principalTable: "Comments",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_Tasks_TaskID",
                table: "Comments",
                column: "TaskID",
                principalTable: "Tasks",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CommentDetails_Comments_CommentID",
                table: "CommentDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_Comments_Tasks_TaskID",
                table: "Comments");

            migrationBuilder.DropIndex(
                name: "IX_Comments_TaskID",
                table: "Comments");

            migrationBuilder.DropIndex(
                name: "IX_CommentDetails_CommentID",
                table: "CommentDetails");
        }
    }
}
