using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace aqua_api.Migrations
{
    /// <inheritdoc />
    public partial class AddBudgetAdjustmentEffectivePeriod : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EffectiveMonth",
                table: "RII_BUDGET_PLAN_FISH_BATCH_ADJUSTMENT",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EffectiveYear",
                table: "RII_BUDGET_PLAN_FISH_BATCH_ADJUSTMENT",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RII_BUDGET_PLAN_FISH_BATCH_ADJUSTMENT_PERIOD",
                table: "RII_BUDGET_PLAN_FISH_BATCH_ADJUSTMENT",
                columns: new[] { "BudgetPlanFishBatchId", "EffectiveYear", "EffectiveMonth" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RII_BUDGET_PLAN_FISH_BATCH_ADJUSTMENT_PERIOD",
                table: "RII_BUDGET_PLAN_FISH_BATCH_ADJUSTMENT");

            migrationBuilder.DropColumn(
                name: "EffectiveMonth",
                table: "RII_BUDGET_PLAN_FISH_BATCH_ADJUSTMENT");

            migrationBuilder.DropColumn(
                name: "EffectiveYear",
                table: "RII_BUDGET_PLAN_FISH_BATCH_ADJUSTMENT");
        }
    }
}
