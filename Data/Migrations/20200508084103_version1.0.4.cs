using Microsoft.EntityFrameworkCore.Migrations;

namespace Data.Migrations
{
    public partial class version104 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_NotificationDetails_NotificationID",
                table: "NotificationDetails",
                column: "NotificationID");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationDetails_UserID",
                table: "NotificationDetails",
                column: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK_NotificationDetails_Notifications_NotificationID",
                table: "NotificationDetails",
                column: "NotificationID",
                principalTable: "Notifications",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NotificationDetails_Users_UserID",
                table: "NotificationDetails",
                column: "UserID",
                principalTable: "Users",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NotificationDetails_Notifications_NotificationID",
                table: "NotificationDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_NotificationDetails_Users_UserID",
                table: "NotificationDetails");

            migrationBuilder.DropIndex(
                name: "IX_NotificationDetails_NotificationID",
                table: "NotificationDetails");

            migrationBuilder.DropIndex(
                name: "IX_NotificationDetails_UserID",
                table: "NotificationDetails");
        }
    }
}
