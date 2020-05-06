using Microsoft.EntityFrameworkCore.Migrations;

namespace Data.Migrations
{
    public partial class AddForeignKeyForTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TaskID",
                table: "Projects",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tutorials_ProjectID",
                table: "Tutorials",
                column: "ProjectID");

            migrationBuilder.CreateIndex(
                name: "IX_Tutorials_TaskID",
                table: "Tutorials",
                column: "TaskID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_OCID",
                table: "Tasks",
                column: "OCID");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_TaskID",
                table: "Projects",
                column: "TaskID");

            migrationBuilder.CreateIndex(
                name: "IX_Histories_TaskID",
                table: "Histories",
                column: "TaskID");

            migrationBuilder.CreateIndex(
                name: "IX_Follows_TaskID",
                table: "Follows",
                column: "TaskID",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Follows_Tasks_TaskID",
                table: "Follows",
                column: "TaskID",
                principalTable: "Tasks",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Histories_Tasks_TaskID",
                table: "Histories",
                column: "TaskID",
                principalTable: "Tasks",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_Tasks_TaskID",
                table: "Projects",
                column: "TaskID",
                principalTable: "Tasks",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_OCs_OCID",
                table: "Tasks",
                column: "OCID",
                principalTable: "OCs",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Tutorials_Projects_ProjectID",
                table: "Tutorials",
                column: "ProjectID",
                principalTable: "Projects",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Tutorials_Tasks_TaskID",
                table: "Tutorials",
                column: "TaskID",
                principalTable: "Tasks",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Follows_Tasks_TaskID",
                table: "Follows");

            migrationBuilder.DropForeignKey(
                name: "FK_Histories_Tasks_TaskID",
                table: "Histories");

            migrationBuilder.DropForeignKey(
                name: "FK_Projects_Tasks_TaskID",
                table: "Projects");

            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_OCs_OCID",
                table: "Tasks");

            migrationBuilder.DropForeignKey(
                name: "FK_Tutorials_Projects_ProjectID",
                table: "Tutorials");

            migrationBuilder.DropForeignKey(
                name: "FK_Tutorials_Tasks_TaskID",
                table: "Tutorials");

            migrationBuilder.DropIndex(
                name: "IX_Tutorials_ProjectID",
                table: "Tutorials");

            migrationBuilder.DropIndex(
                name: "IX_Tutorials_TaskID",
                table: "Tutorials");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_OCID",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_Projects_TaskID",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Histories_TaskID",
                table: "Histories");

            migrationBuilder.DropIndex(
                name: "IX_Follows_TaskID",
                table: "Follows");

            migrationBuilder.DropColumn(
                name: "TaskID",
                table: "Projects");
        }
    }
}
