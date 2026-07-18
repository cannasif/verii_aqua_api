using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace aqua_api.Migrations
{
    /// <inheritdoc />
    public partial class ExtendBudgetFishPriceDimensions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_RII_BUDGET_PLAN_FISH_PRICE_PERIOD_ACTIVE",
                table: "RII_BUDGET_PLAN_FISH_PRICE");

            migrationBuilder.DropCheckConstraint(
                name: "CK_RII_BUDGET_PLAN_FISH_PRICE_NON_NEGATIVE",
                table: "RII_BUDGET_PLAN_FISH_PRICE");

            migrationBuilder.RenameColumn(
                name: "UnitPriceEuro",
                table: "RII_BUDGET_PLAN_FISH_PRICE",
                newName: "UnitPrice");

            migrationBuilder.AddColumn<string>(
                name: "CurrencyCode",
                table: "RII_BUDGET_PLAN_FISH_PRICE",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "EUR");

            migrationBuilder.AddColumn<int>(
                name: "IncreasePeriodMonths",
                table: "RII_BUDGET_PLAN_FISH_PRICE",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<decimal>(
                name: "IncreaseRatePercent",
                table: "RII_BUDGET_PLAN_FISH_PRICE",
                type: "decimal(18,6)",
                precision: 18,
                scale: 6,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<byte>(
                name: "MarketType",
                table: "RII_BUDGET_PLAN_FISH_PRICE",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<byte>(
                name: "PriceType",
                table: "RII_BUDGET_PLAN_FISH_PRICE",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)1);

            migrationBuilder.CreateIndex(
                name: "UX_RII_BUDGET_PLAN_FISH_PRICE_PERIOD_ACTIVE",
                table: "RII_BUDGET_PLAN_FISH_PRICE",
                columns: new[] { "BudgetPlanId", "FishStockId", "CalibrationDefinitionId", "Year", "Month", "PriceType", "MarketType", "CurrencyCode" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_RII_BUDGET_PLAN_FISH_PRICE_MARKET_TYPE",
                table: "RII_BUDGET_PLAN_FISH_PRICE",
                sql: "[MarketType] IN (0, 1)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_RII_BUDGET_PLAN_FISH_PRICE_NON_NEGATIVE",
                table: "RII_BUDGET_PLAN_FISH_PRICE",
                sql: "[UnitPrice] >= 0 AND [IncreaseRatePercent] >= 0 AND [IncreasePeriodMonths] >= 1");

            migrationBuilder.AddCheckConstraint(
                name: "CK_RII_BUDGET_PLAN_FISH_PRICE_PRICE_TYPE",
                table: "RII_BUDGET_PLAN_FISH_PRICE",
                sql: "[PriceType] IN (0, 1)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_RII_BUDGET_PLAN_FISH_PRICE_PERIOD_ACTIVE",
                table: "RII_BUDGET_PLAN_FISH_PRICE");

            migrationBuilder.DropCheckConstraint(
                name: "CK_RII_BUDGET_PLAN_FISH_PRICE_MARKET_TYPE",
                table: "RII_BUDGET_PLAN_FISH_PRICE");

            migrationBuilder.DropCheckConstraint(
                name: "CK_RII_BUDGET_PLAN_FISH_PRICE_NON_NEGATIVE",
                table: "RII_BUDGET_PLAN_FISH_PRICE");

            migrationBuilder.DropCheckConstraint(
                name: "CK_RII_BUDGET_PLAN_FISH_PRICE_PRICE_TYPE",
                table: "RII_BUDGET_PLAN_FISH_PRICE");

            migrationBuilder.DropColumn(
                name: "CurrencyCode",
                table: "RII_BUDGET_PLAN_FISH_PRICE");

            migrationBuilder.DropColumn(
                name: "IncreasePeriodMonths",
                table: "RII_BUDGET_PLAN_FISH_PRICE");

            migrationBuilder.DropColumn(
                name: "IncreaseRatePercent",
                table: "RII_BUDGET_PLAN_FISH_PRICE");

            migrationBuilder.DropColumn(
                name: "MarketType",
                table: "RII_BUDGET_PLAN_FISH_PRICE");

            migrationBuilder.DropColumn(
                name: "PriceType",
                table: "RII_BUDGET_PLAN_FISH_PRICE");

            migrationBuilder.RenameColumn(
                name: "UnitPrice",
                table: "RII_BUDGET_PLAN_FISH_PRICE",
                newName: "UnitPriceEuro");

            migrationBuilder.CreateIndex(
                name: "UX_RII_BUDGET_PLAN_FISH_PRICE_PERIOD_ACTIVE",
                table: "RII_BUDGET_PLAN_FISH_PRICE",
                columns: new[] { "BudgetPlanId", "FishStockId", "CalibrationDefinitionId", "Year", "Month" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_RII_BUDGET_PLAN_FISH_PRICE_NON_NEGATIVE",
                table: "RII_BUDGET_PLAN_FISH_PRICE",
                sql: "[UnitPriceEuro] >= 0");
        }
    }
}
