using Microsoft.EntityFrameworkCore.Migrations;

namespace Data.Migrations
{
    public partial class AddForeignKeyForTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Managers_Projects_ProjectID",
                table: "Managers");

            migrationBuilder.DropForeignKey(
                name: "FK_Tags_Tasks_TaskID",
                table: "Tags");

            migrationBuilder.DropForeignKey(
                name: "FK_TeamMembers_Projects_ProjectID",
                table: "TeamMembers");

            migrationBuilder.DropIndex(
                name: "IX_Tags_UserID",
                table: "Tags");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Follows",
                table: "Follows");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Deputies",
                table: "Deputies");

            migrationBuilder.DropColumn(
                name: "ID",
                table: "Follows");

            migrationBuilder.DropColumn(
                name: "ID",
                table: "Deputies");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Follows",
                table: "Follows",
                columns: new[] { "TaskID", "UserID" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Deputies",
                table: "Deputies",
                columns: new[] { "TaskID", "UserID" });

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
                name: "IX_Tasks_ProjectID",
                table: "Tasks",
                column: "ProjectID");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_UserID",
                table: "Tags",
                column: "UserID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Histories_TaskID",
                table: "Histories",
                column: "TaskID");

            migrationBuilder.CreateIndex(
                name: "IX_Follows_UserID",
                table: "Follows",
                column: "UserID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Deputies_UserID",
                table: "Deputies",
                column: "UserID",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Deputies_Tasks_TaskID",
                table: "Deputies",
                column: "TaskID",
                principalTable: "Tasks",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_Deputies_Users_UserID",
                table: "Deputies",
                column: "UserID",
                principalTable: "Users",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Follows_Tasks_TaskID",
                table: "Follows",
                column: "TaskID",
                principalTable: "Tasks",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_Follows_Users_UserID",
                table: "Follows",
                column: "UserID",
                principalTable: "Users",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_Histories_Tasks_TaskID",
                table: "Histories",
                column: "TaskID",
                principalTable: "Tasks",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Managers_Projects_ProjectID",
                table: "Managers",
                column: "ProjectID",
                principalTable: "Projects",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_Tags_Tasks_TaskID",
                table: "Tags",
                column: "TaskID",
                principalTable: "Tasks",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_OCs_OCID",
                table: "Tasks",
                column: "OCID",
                principalTable: "OCs",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_Projects_ProjectID",
                table: "Tasks",
                column: "ProjectID",
                principalTable: "Projects",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_TeamMembers_Projects_ProjectID",
                table: "TeamMembers",
                column: "ProjectID",
                principalTable: "Projects",
                principalColumn: "ID");

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
                principalColumn: "ID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Deputies_Tasks_TaskID",
                table: "Deputies");

            migrationBuilder.DropForeignKey(
                name: "FK_Deputies_Users_UserID",
                table: "Deputies");

            migrationBuilder.DropForeignKey(
                name: "FK_Follows_Tasks_TaskID",
                table: "Follows");

            migrationBuilder.DropForeignKey(
                name: "FK_Follows_Users_UserID",
                table: "Follows");

            migrationBuilder.DropForeignKey(
                name: "FK_Histories_Tasks_TaskID",
                table: "Histories");

            migrationBuilder.DropForeignKey(
                name: "FK_Managers_Projects_ProjectID",
                table: "Managers");

            migrationBuilder.DropForeignKey(
                name: "FK_Tags_Tasks_TaskID",
                table: "Tags");

            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_OCs_OCID",
                table: "Tasks");

            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_Projects_ProjectID",
                table: "Tasks");

            migrationBuilder.DropForeignKey(
                name: "FK_TeamMembers_Projects_ProjectID",
                table: "TeamMembers");

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
                name: "IX_Tasks_ProjectID",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_Tags_UserID",
                table: "Tags");

            migrationBuilder.DropIndex(
                name: "IX_Histories_TaskID",
                table: "Histories");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Follows",
                table: "Follows");

            migrationBuilder.DropIndex(
                name: "IX_Follows_UserID",
                table: "Follows");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Deputies",
                table: "Deputies");

            migrationBuilder.DropIndex(
                name: "IX_Deputies_UserID",
                table: "Deputies");

            migrationBuilder.AddColumn<int>(
                name: "ID",
                table: "Follows",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<int>(
                name: "ID",
                table: "Deputies",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Follows",
                table: "Follows",
                column: "ID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Deputies",
                table: "Deputies",
                column: "ID");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_UserID",
                table: "Tags",
                column: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK_Managers_Projects_ProjectID",
                table: "Managers",
                column: "ProjectID",
                principalTable: "Projects",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Tags_Tasks_TaskID",
                table: "Tags",
                column: "TaskID",
                principalTable: "Tasks",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TeamMembers_Projects_ProjectID",
                table: "TeamMembers",
                column: "ProjectID",
                principalTable: "Projects",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
