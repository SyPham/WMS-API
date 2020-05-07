using Microsoft.EntityFrameworkCore.Migrations;

namespace Data.Migrations
{
    public partial class version100 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Tags",
                table: "Tags");

            migrationBuilder.DropIndex(
                name: "IX_Tags_TaskID_UserID",
                table: "Tags");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Deputies",
                table: "Deputies");

            migrationBuilder.AddColumn<int>(
                name: "ID",
                table: "Tags",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<int>(
                name: "ID",
                table: "Deputies",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Tags",
                table: "Tags",
                column: "ID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Deputies",
                table: "Deputies",
                column: "ID");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_TaskID",
                table: "Tags",
                column: "TaskID");

            migrationBuilder.CreateIndex(
                name: "IX_Deputies_TaskID",
                table: "Deputies",
                column: "TaskID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Tags",
                table: "Tags");

            migrationBuilder.DropIndex(
                name: "IX_Tags_TaskID",
                table: "Tags");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Deputies",
                table: "Deputies");

            migrationBuilder.DropIndex(
                name: "IX_Deputies_TaskID",
                table: "Deputies");

            migrationBuilder.DropColumn(
                name: "ID",
                table: "Tags");

            migrationBuilder.DropColumn(
                name: "ID",
                table: "Deputies");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Tags",
                table: "Tags",
                columns: new[] { "TaskID", "UserID" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Deputies",
                table: "Deputies",
                columns: new[] { "TaskID", "UserID" });

            migrationBuilder.CreateIndex(
                name: "IX_Tags_TaskID_UserID",
                table: "Tags",
                columns: new[] { "TaskID", "UserID" });
        }
    }
}
