using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace aqua_api.Migrations
{
    /// <inheritdoc />
    public partial class UpdateBudgetPlanningWorkflowAndAdjustmentTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_RII_BUDGET_PlanFishBatchAdjustment_Type",
                table: "RII_BUDGET_PlanFishBatchAdjustment");

            migrationBuilder.AddCheckConstraint(
                name: "CK_RII_BUDGET_PlanFishBatchAdjustment_Type",
                table: "RII_BUDGET_PlanFishBatchAdjustment",
                sql: "[AdjustmentType] IN (0,1,2,3)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_RII_BUDGET_PlanFishBatchAdjustment_Type",
                table: "RII_BUDGET_PlanFishBatchAdjustment");

            migrationBuilder.AddCheckConstraint(
                name: "CK_RII_BUDGET_PlanFishBatchAdjustment_Type",
                table: "RII_BUDGET_PlanFishBatchAdjustment",
                sql: "[AdjustmentType] IN (0,1)");
        }
    }
}
