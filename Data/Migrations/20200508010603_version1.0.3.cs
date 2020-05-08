using Microsoft.EntityFrameworkCore.Migrations;

namespace Data.Migrations
{
    public partial class version103 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tutorials_Projects_ProjectID",
                table: "Tutorials");

            migrationBuilder.DropIndex(
                name: "IX_Tutorials_TaskID",
                table: "Tutorials");

            migrationBuilder.AlterColumn<int>(
                name: "TaskID",
                table: "Tutorials",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "ProjectID",
                table: "Tutorials",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "IX_Tutorials_TaskID",
                table: "Tutorials",
                column: "TaskID",
                unique: true,
                filter: "[TaskID] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Tutorials_Projects_ProjectID",
                table: "Tutorials",
                column: "ProjectID",
                principalTable: "Projects",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tutorials_Projects_ProjectID",
                table: "Tutorials");

            migrationBuilder.DropIndex(
                name: "IX_Tutorials_TaskID",
                table: "Tutorials");

            migrationBuilder.AlterColumn<int>(
                name: "TaskID",
                table: "Tutorials",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ProjectID",
                table: "Tutorials",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tutorials_TaskID",
                table: "Tutorials",
                column: "TaskID",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Tutorials_Projects_ProjectID",
                table: "Tutorials",
                column: "ProjectID",
                principalTable: "Projects",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
