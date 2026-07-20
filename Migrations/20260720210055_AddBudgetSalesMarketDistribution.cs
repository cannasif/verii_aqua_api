using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace aqua_api.Migrations
{
    /// <inheritdoc />
    public partial class AddBudgetSalesMarketDistribution : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RII_BUDGET_PLAN_SALES_LINE_PERIOD_ACTIVE",
                table: "RII_BUDGET_PLAN_SALES_LINE");

            migrationBuilder.AddColumn<string>(
                name: "CurrencyCode",
                table: "RII_BUDGET_PLAN_SALES_LINE",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "EUR");

            migrationBuilder.AddColumn<byte>(
                name: "MarketType",
                table: "RII_BUDGET_PLAN_SALES_LINE",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<decimal>(
                name: "SourceUnitPrice",
                table: "RII_BUDGET_PLAN_SALES_LINE",
                type: "decimal(18,6)",
                nullable: true);

            migrationBuilder.Sql(
                "UPDATE [RII_BUDGET_PLAN_SALES_LINE] SET [CurrencyCode] = 'EUR', [SourceUnitPrice] = [UnitPrice]");

            migrationBuilder.CreateTable(
                name: "RII_BUDGET_PLAN_SALES_DISTRIBUTION",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BudgetPlanId = table.Column<long>(type: "bigint", nullable: false),
                    BudgetPlanMonthlyProjectionId = table.Column<long>(type: "bigint", nullable: false),
                    BudgetPlanSalesLineId = table.Column<long>(type: "bigint", nullable: false),
                    BudgetPlanFishBatchId = table.Column<long>(type: "bigint", nullable: false),
                    CalibrationDefinitionId = table.Column<long>(type: "bigint", nullable: true),
                    Year = table.Column<int>(type: "int", nullable: false),
                    Month = table.Column<int>(type: "int", nullable: false),
                    MarketType = table.Column<byte>(type: "tinyint", nullable: false),
                    SalesTon = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    SalesKg = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    SalesCount = table.Column<int>(type: "int", nullable: false),
                    UnitGram = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    CurrencyCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    UnitPriceEuro = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    ExchangeRate = table.Column<decimal>(type: "decimal(18,6)", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    AmountEuro = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    AmountTry = table.Column<decimal>(type: "decimal(18,6)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: true),
                    UpdatedBy = table.Column<long>(type: "bigint", nullable: true),
                    DeletedBy = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RII_BUDGET_PLAN_SALES_DISTRIBUTION", x => x.Id);
                    table.CheckConstraint("CK_RII_BUDGET_PLAN_SALES_DISTRIBUTION_MARKET_TYPE", "[MarketType] IN (0, 1)");
                    table.CheckConstraint("CK_RII_BUDGET_PLAN_SALES_DISTRIBUTION_MONTH", "[Month] BETWEEN 1 AND 12");
                    table.CheckConstraint("CK_RII_BUDGET_PLAN_SALES_DISTRIBUTION_NON_NEGATIVE", "[SalesTon] >= 0 AND [SalesKg] >= 0 AND [SalesCount] >= 0");
                    table.ForeignKey(
                        name: "FK_RII_BUDGET_PLAN_SALES_DISTRIBUTION_RII_BUDGET_CALIBRATION_DEFINITION_CalibrationDefinitionId",
                        column: x => x.CalibrationDefinitionId,
                        principalTable: "RII_BUDGET_CALIBRATION_DEFINITION",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_BUDGET_PLAN_SALES_DISTRIBUTION_RII_BUDGET_PLAN_BudgetPlanId",
                        column: x => x.BudgetPlanId,
                        principalTable: "RII_BUDGET_PLAN",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RII_BUDGET_PLAN_SALES_DISTRIBUTION_RII_BUDGET_PLAN_FISH_BATCH_BudgetPlanFishBatchId",
                        column: x => x.BudgetPlanFishBatchId,
                        principalTable: "RII_BUDGET_PLAN_FISH_BATCH",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_BUDGET_PLAN_SALES_DISTRIBUTION_RII_BUDGET_PLAN_MONTHLY_PROJECTION_BudgetPlanMonthlyProjectionId",
                        column: x => x.BudgetPlanMonthlyProjectionId,
                        principalTable: "RII_BUDGET_PLAN_MONTHLY_PROJECTION",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_BUDGET_PLAN_SALES_DISTRIBUTION_RII_BUDGET_PLAN_SALES_LINE_BudgetPlanSalesLineId",
                        column: x => x.BudgetPlanSalesLineId,
                        principalTable: "RII_BUDGET_PLAN_SALES_LINE",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_BUDGET_PLAN_SALES_DISTRIBUTION_RII_USERS_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "RII_USERS",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_BUDGET_PLAN_SALES_DISTRIBUTION_RII_USERS_DeletedBy",
                        column: x => x.DeletedBy,
                        principalTable: "RII_USERS",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_BUDGET_PLAN_SALES_DISTRIBUTION_RII_USERS_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "RII_USERS",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_RII_BUDGET_PLAN_SALES_LINE_PERIOD_ACTIVE",
                table: "RII_BUDGET_PLAN_SALES_LINE",
                columns: new[] { "BudgetPlanId", "BudgetPlanFishBatchId", "Year", "Month", "MarketType" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_RII_BUDGET_PLAN_SALES_LINE_MARKET_TYPE",
                table: "RII_BUDGET_PLAN_SALES_LINE",
                sql: "[MarketType] IN (0, 1)");

            migrationBuilder.CreateIndex(
                name: "IX_RII_BUDGET_PLAN_SALES_DISTRIBUTION_BudgetPlanFishBatchId",
                table: "RII_BUDGET_PLAN_SALES_DISTRIBUTION",
                column: "BudgetPlanFishBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_RII_BUDGET_PLAN_SALES_DISTRIBUTION_BudgetPlanMonthlyProjectionId",
                table: "RII_BUDGET_PLAN_SALES_DISTRIBUTION",
                column: "BudgetPlanMonthlyProjectionId");

            migrationBuilder.CreateIndex(
                name: "IX_RII_BUDGET_PLAN_SALES_DISTRIBUTION_BudgetPlanSalesLineId",
                table: "RII_BUDGET_PLAN_SALES_DISTRIBUTION",
                column: "BudgetPlanSalesLineId");

            migrationBuilder.CreateIndex(
                name: "IX_RII_BUDGET_PLAN_SALES_DISTRIBUTION_CalibrationDefinitionId",
                table: "RII_BUDGET_PLAN_SALES_DISTRIBUTION",
                column: "CalibrationDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_RII_BUDGET_PLAN_SALES_DISTRIBUTION_CreatedBy",
                table: "RII_BUDGET_PLAN_SALES_DISTRIBUTION",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_RII_BUDGET_PLAN_SALES_DISTRIBUTION_DeletedBy",
                table: "RII_BUDGET_PLAN_SALES_DISTRIBUTION",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_RII_BUDGET_PLAN_SALES_DISTRIBUTION_DIMENSION_ACTIVE",
                table: "RII_BUDGET_PLAN_SALES_DISTRIBUTION",
                columns: new[] { "BudgetPlanId", "BudgetPlanFishBatchId", "Year", "Month", "MarketType", "CalibrationDefinitionId" },
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_RII_BUDGET_PLAN_SALES_DISTRIBUTION_UpdatedBy",
                table: "RII_BUDGET_PLAN_SALES_DISTRIBUTION",
                column: "UpdatedBy");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RII_BUDGET_PLAN_SALES_DISTRIBUTION");

            migrationBuilder.DropIndex(
                name: "IX_RII_BUDGET_PLAN_SALES_LINE_PERIOD_ACTIVE",
                table: "RII_BUDGET_PLAN_SALES_LINE");

            migrationBuilder.DropCheckConstraint(
                name: "CK_RII_BUDGET_PLAN_SALES_LINE_MARKET_TYPE",
                table: "RII_BUDGET_PLAN_SALES_LINE");

            migrationBuilder.DropColumn(
                name: "CurrencyCode",
                table: "RII_BUDGET_PLAN_SALES_LINE");

            migrationBuilder.DropColumn(
                name: "MarketType",
                table: "RII_BUDGET_PLAN_SALES_LINE");

            migrationBuilder.DropColumn(
                name: "SourceUnitPrice",
                table: "RII_BUDGET_PLAN_SALES_LINE");

            migrationBuilder.CreateIndex(
                name: "IX_RII_BUDGET_PLAN_SALES_LINE_PERIOD_ACTIVE",
                table: "RII_BUDGET_PLAN_SALES_LINE",
                columns: new[] { "BudgetPlanId", "BudgetPlanFishBatchId", "Year", "Month" },
                filter: "[IsDeleted] = 0");
        }
    }
}
