using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace aqua_api.Migrations
{
    /// <inheritdoc />
    public partial class AddMortalityBiomassCalculationMode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MortalityBiomassCalculationMode",
                table: "RII_AQUA_SETTING",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddCheckConstraint(
                name: "CK_RII_AQUA_SETTING_MORTALITY_BIOMASS_CALCULATION_MODE",
                table: "RII_AQUA_SETTING",
                sql: "[MortalityBiomassCalculationMode] IN (0,1)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_RII_AQUA_SETTING_MORTALITY_BIOMASS_CALCULATION_MODE",
                table: "RII_AQUA_SETTING");

            migrationBuilder.DropColumn(
                name: "MortalityBiomassCalculationMode",
                table: "RII_AQUA_SETTING");
        }
    }
}
