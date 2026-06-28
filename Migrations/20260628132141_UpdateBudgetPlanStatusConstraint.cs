using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace aqua_api.Migrations
{
    /// <inheritdoc />
    public partial class UpdateBudgetPlanStatusConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_RII_BUDGET_Plan_Status",
                table: "RII_BUDGET_Plan");

            migrationBuilder.AddCheckConstraint(
                name: "CK_RII_BUDGET_Plan_Status",
                table: "RII_BUDGET_Plan",
                sql: "[Status] IN (0,1,2,3,4,5,6,7)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_RII_BUDGET_Plan_Status",
                table: "RII_BUDGET_Plan");

            migrationBuilder.AddCheckConstraint(
                name: "CK_RII_BUDGET_Plan_Status",
                table: "RII_BUDGET_Plan",
                sql: "[Status] IN (0,1,2,3)");
        }
    }
}
