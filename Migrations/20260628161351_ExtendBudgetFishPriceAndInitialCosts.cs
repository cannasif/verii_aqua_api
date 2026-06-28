using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace aqua_api.Migrations
{
    /// <inheritdoc />
    public partial class ExtendBudgetFishPriceAndInitialCosts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_RII_BUDGET_PLAN_FISH_PRICE_PERIOD_ACTIVE",
                table: "RII_BUDGET_PLAN_FISH_PRICE");

            migrationBuilder.AddColumn<long>(
                name: "FishStockId",
                table: "RII_BUDGET_PLAN_FISH_PRICE",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "InitialSmmAmount",
                table: "RII_BUDGET_PLAN_FISH_BATCH",
                type: "decimal(18,6)",
                precision: 18,
                scale: 6,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "InitialUnitCost",
                table: "RII_BUDGET_PLAN_FISH_BATCH",
                type: "decimal(18,6)",
                precision: 18,
                scale: 6,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RII_BUDGET_PLAN_FISH_PRICE_FishStockId",
                table: "RII_BUDGET_PLAN_FISH_PRICE",
                column: "FishStockId");

            migrationBuilder.CreateIndex(
                name: "UX_RII_BUDGET_PLAN_FISH_PRICE_PERIOD_ACTIVE",
                table: "RII_BUDGET_PLAN_FISH_PRICE",
                columns: new[] { "BudgetPlanId", "FishStockId", "CalibrationDefinitionId", "Year", "Month" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.AddForeignKey(
                name: "FK_RII_BUDGET_PLAN_FISH_PRICE_RII_STOCK_FishStockId",
                table: "RII_BUDGET_PLAN_FISH_PRICE",
                column: "FishStockId",
                principalTable: "RII_STOCK",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RII_BUDGET_PLAN_FISH_PRICE_RII_STOCK_FishStockId",
                table: "RII_BUDGET_PLAN_FISH_PRICE");

            migrationBuilder.DropIndex(
                name: "IX_RII_BUDGET_PLAN_FISH_PRICE_FishStockId",
                table: "RII_BUDGET_PLAN_FISH_PRICE");

            migrationBuilder.DropIndex(
                name: "UX_RII_BUDGET_PLAN_FISH_PRICE_PERIOD_ACTIVE",
                table: "RII_BUDGET_PLAN_FISH_PRICE");

            migrationBuilder.DropColumn(
                name: "FishStockId",
                table: "RII_BUDGET_PLAN_FISH_PRICE");

            migrationBuilder.DropColumn(
                name: "InitialSmmAmount",
                table: "RII_BUDGET_PLAN_FISH_BATCH");

            migrationBuilder.DropColumn(
                name: "InitialUnitCost",
                table: "RII_BUDGET_PLAN_FISH_BATCH");

            migrationBuilder.CreateIndex(
                name: "UX_RII_BUDGET_PLAN_FISH_PRICE_PERIOD_ACTIVE",
                table: "RII_BUDGET_PLAN_FISH_PRICE",
                columns: new[] { "BudgetPlanId", "CalibrationDefinitionId", "Year", "Month" },
                unique: true,
                filter: "[IsDeleted] = 0");
        }
    }
}
