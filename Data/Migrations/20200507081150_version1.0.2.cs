using Microsoft.EntityFrameworkCore.Migrations;

namespace Data.Migrations
{
    public partial class version102 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_TeamMembers",
                table: "TeamMembers");

            migrationBuilder.DropIndex(
                name: "IX_TeamMembers_UserID",
                table: "TeamMembers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Managers",
                table: "Managers");

            migrationBuilder.DropIndex(
                name: "IX_Managers_UserID",
                table: "Managers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Follows",
                table: "Follows");

            migrationBuilder.DropIndex(
                name: "IX_Follows_UserID",
                table: "Follows");

            migrationBuilder.AddColumn<int>(
                name: "ID",
                table: "TeamMembers",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<int>(
                name: "ID",
                table: "Managers",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<int>(
                name: "ID",
                table: "Follows",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TeamMembers",
                table: "TeamMembers",
                column: "ID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Managers",
                table: "Managers",
                column: "ID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Follows",
                table: "Follows",
                column: "ID");

            migrationBuilder.CreateIndex(
                name: "IX_TeamMembers_UserID",
                table: "TeamMembers",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_Managers_UserID",
                table: "Managers",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_Follows_TaskID",
                table: "Follows",
                column: "TaskID");

            migrationBuilder.CreateIndex(
                name: "IX_Follows_UserID",
                table: "Follows",
                column: "UserID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_TeamMembers",
                table: "TeamMembers");

            migrationBuilder.DropIndex(
                name: "IX_TeamMembers_UserID",
                table: "TeamMembers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Managers",
                table: "Managers");

            migrationBuilder.DropIndex(
                name: "IX_Managers_UserID",
                table: "Managers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Follows",
                table: "Follows");

            migrationBuilder.DropIndex(
                name: "IX_Follows_TaskID",
                table: "Follows");

            migrationBuilder.DropIndex(
                name: "IX_Follows_UserID",
                table: "Follows");

            migrationBuilder.DropColumn(
                name: "ID",
                table: "TeamMembers");

            migrationBuilder.DropColumn(
                name: "ID",
                table: "Managers");

            migrationBuilder.DropColumn(
                name: "ID",
                table: "Follows");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TeamMembers",
                table: "TeamMembers",
                columns: new[] { "UserID", "ProjectID" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Managers",
                table: "Managers",
                columns: new[] { "UserID", "ProjectID" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Follows",
                table: "Follows",
                columns: new[] { "TaskID", "UserID" });

            migrationBuilder.CreateIndex(
                name: "IX_TeamMembers_UserID",
                table: "TeamMembers",
                column: "UserID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Managers_UserID",
                table: "Managers",
                column: "UserID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Follows_UserID",
                table: "Follows",
                column: "UserID",
                unique: true);
        }
    }
}
