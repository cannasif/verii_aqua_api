using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace aqua_api.Migrations
{
    /// <inheritdoc />
    public partial class DecoupleWeatherSeverityFromType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RII_WeatherSeverity_RII_WeatherType_WeatherTypeId",
                table: "RII_WeatherSeverity");

            migrationBuilder.DropIndex(
                name: "UX_RII_WeatherSeverity_WeatherType_Code_Active",
                table: "RII_WeatherSeverity");

            migrationBuilder.DropIndex(
                name: "IX_RII_WeatherSeverity_WeatherTypeId",
                table: "RII_WeatherSeverity");

            migrationBuilder.DropColumn(
                name: "WeatherTypeId",
                table: "RII_WeatherSeverity");

            migrationBuilder.CreateIndex(
                name: "IX_RII_WeatherSeverity_Code_Active",
                table: "RII_WeatherSeverity",
                column: "Code",
                filter: "[IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RII_WeatherSeverity_Code_Active",
                table: "RII_WeatherSeverity");

            migrationBuilder.AddColumn<long>(
                name: "WeatherTypeId",
                table: "RII_WeatherSeverity",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RII_WeatherSeverity_WeatherTypeId",
                table: "RII_WeatherSeverity",
                column: "WeatherTypeId");

            migrationBuilder.CreateIndex(
                name: "UX_RII_WeatherSeverity_WeatherType_Code_Active",
                table: "RII_WeatherSeverity",
                columns: new[] { "WeatherTypeId", "Code" },
                unique: true,
                filter: "[IsDeleted] = 0 AND [WeatherTypeId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_RII_WeatherSeverity_RII_WeatherType_WeatherTypeId",
                table: "RII_WeatherSeverity",
                column: "WeatherTypeId",
                principalTable: "RII_WeatherType",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
