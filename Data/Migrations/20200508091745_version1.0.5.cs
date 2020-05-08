using Microsoft.EntityFrameworkCore.Migrations;

namespace Data.Migrations
{
    public partial class version105 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_NotificationDetails_NotificationID",
                table: "NotificationDetails");

            migrationBuilder.AlterColumn<int>(
                name: "UserID",
                table: "Notifications",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserID",
                table: "Notifications",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationDetails_NotificationID",
                table: "NotificationDetails",
                column: "NotificationID",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Users_UserID",
                table: "Notifications",
                column: "UserID",
                principalTable: "Users",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Users_UserID",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_UserID",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_NotificationDetails_NotificationID",
                table: "NotificationDetails");

            migrationBuilder.AlterColumn<int>(
                name: "UserID",
                table: "Notifications",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_NotificationDetails_NotificationID",
                table: "NotificationDetails",
                column: "NotificationID");
        }
    }
}
