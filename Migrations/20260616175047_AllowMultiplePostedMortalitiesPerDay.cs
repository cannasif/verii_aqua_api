using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace aqua_api.Migrations
{
    /// <inheritdoc />
    public partial class AllowMultiplePostedMortalitiesPerDay : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_RII_Mortality_ProjectDate_Active",
                table: "RII_Mortality");

            migrationBuilder.CreateIndex(
                name: "UX_RII_Mortality_ProjectDate_Active",
                table: "RII_Mortality",
                columns: new[] { "ProjectId", "MortalityDate" },
                unique: true,
                filter: "[IsDeleted] = 0 AND [Status] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_RII_Mortality_ProjectDate_Active",
                table: "RII_Mortality");

            migrationBuilder.CreateIndex(
                name: "UX_RII_Mortality_ProjectDate_Active",
                table: "RII_Mortality",
                columns: new[] { "ProjectId", "MortalityDate" },
                unique: true,
                filter: "[IsDeleted] = 0");
        }
    }
}
