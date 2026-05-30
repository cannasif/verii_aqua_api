using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace aqua_api.Migrations
{
    /// <inheritdoc />
    public partial class AddUserManagerRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "ManagerUserId",
                table: "RII_USERS",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_ManagerUserId",
                table: "RII_USERS",
                column: "ManagerUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_RII_USERS_RII_USERS_ManagerUserId",
                table: "RII_USERS",
                column: "ManagerUserId",
                principalTable: "RII_USERS",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RII_USERS_RII_USERS_ManagerUserId",
                table: "RII_USERS");

            migrationBuilder.DropIndex(
                name: "IX_Users_ManagerUserId",
                table: "RII_USERS");

            migrationBuilder.DropColumn(
                name: "ManagerUserId",
                table: "RII_USERS");
        }
    }
}
