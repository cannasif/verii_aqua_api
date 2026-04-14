using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace aqua_api.Migrations
{
    /// <inheritdoc />
    public partial class AllowProjectMerge_RequireFullTransfer_AppSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AllowProjectMerge",
                table: "RII_AquaSetting",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RequireFullTransfer",
                table: "RII_AquaSetting",
                type: "bit",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AllowProjectMerge",
                table: "RII_AquaSetting");

            migrationBuilder.DropColumn(
                name: "RequireFullTransfer",
                table: "RII_AquaSetting");
        }
    }
}
