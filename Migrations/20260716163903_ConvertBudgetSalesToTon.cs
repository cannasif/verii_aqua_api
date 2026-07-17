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
            DropSalesLineNonNegativeConstraint(migrationBuilder);

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
            DropSalesLineNonNegativeConstraint(migrationBuilder);

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

        private static void DropSalesLineNonNegativeConstraint(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DECLARE @ConstraintName sysname;
                DECLARE @Sql nvarchar(max);

                SELECT TOP (1) @ConstraintName = cc.[name]
                FROM sys.check_constraints AS cc
                INNER JOIN sys.tables AS t ON t.[object_id] = cc.[parent_object_id]
                INNER JOIN sys.schemas AS s ON s.[schema_id] = t.[schema_id]
                WHERE s.[name] = N'dbo'
                  AND t.[name] = N'RII_BUDGET_PLAN_SALES_LINE'
                  AND (cc.[definition] LIKE N'%SalesKg%' OR cc.[definition] LIKE N'%SalesTon%');

                IF @ConstraintName IS NOT NULL
                BEGIN
                    SET @Sql = N'ALTER TABLE [dbo].[RII_BUDGET_PLAN_SALES_LINE] DROP CONSTRAINT ' + QUOTENAME(@ConstraintName) + N';';
                    EXEC sys.sp_executesql @Sql;
                END;
                """);
        }
    }
}
