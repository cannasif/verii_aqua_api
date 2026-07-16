using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace aqua_api.Migrations
{
    /// <inheritdoc />
    public partial class ConvertBudgetSalesToTon : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_RII_BUDGET_PLAN_SALES_LINE_NON_NEGATIVE",
                table: "RII_BUDGET_PLAN_SALES_LINE");

            migrationBuilder.RenameColumn(
                name: "SalesKg",
                table: "RII_BUDGET_PLAN_SALES_LINE",
                newName: "SalesTon");

            migrationBuilder.RenameColumn(
                name: "SalesKg",
                table: "RII_BUDGET_PLAN_MONTHLY_PROJECTION",
                newName: "SalesTon");

            migrationBuilder.Sql(
                "UPDATE [RII_BUDGET_PLAN_SALES_LINE] SET [SalesTon] = [SalesTon] / 1000.0;");
            migrationBuilder.Sql(
                "UPDATE [RII_BUDGET_PLAN_MONTHLY_PROJECTION] SET [SalesTon] = [SalesTon] / 1000.0;");

            migrationBuilder.AddCheckConstraint(
                name: "CK_RII_BUDGET_PLAN_SALES_LINE_NON_NEGATIVE",
                table: "RII_BUDGET_PLAN_SALES_LINE",
                sql: "[SalesTon] >= 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_RII_BUDGET_PLAN_SALES_LINE_NON_NEGATIVE",
                table: "RII_BUDGET_PLAN_SALES_LINE");

            migrationBuilder.Sql(
                "UPDATE [RII_BUDGET_PLAN_SALES_LINE] SET [SalesTon] = [SalesTon] * 1000.0;");
            migrationBuilder.Sql(
                "UPDATE [RII_BUDGET_PLAN_MONTHLY_PROJECTION] SET [SalesTon] = [SalesTon] * 1000.0;");

            migrationBuilder.RenameColumn(
                name: "SalesTon",
                table: "RII_BUDGET_PLAN_SALES_LINE",
                newName: "SalesKg");

            migrationBuilder.RenameColumn(
                name: "SalesTon",
                table: "RII_BUDGET_PLAN_MONTHLY_PROJECTION",
                newName: "SalesKg");

            migrationBuilder.AddCheckConstraint(
                name: "CK_RII_BUDGET_PLAN_SALES_LINE_NON_NEGATIVE",
                table: "RII_BUDGET_PLAN_SALES_LINE",
                sql: "[SalesKg] >= 0");
        }
    }
}
