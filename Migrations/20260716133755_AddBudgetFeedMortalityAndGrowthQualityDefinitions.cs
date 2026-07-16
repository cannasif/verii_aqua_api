using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace aqua_api.Migrations
{
    public partial class AddBudgetFeedMortalityAndGrowthQualityDefinitions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_RII_BUDGET_PLAN_FEEDING_LINE_NON_NEGATIVE",
                table: "RII_BUDGET_PLAN_FEEDING_LINE");

            migrationBuilder.AddColumn<decimal>(
                name: "FeedMortalityReductionKg",
                table: "RII_BUDGET_PLAN_MONTHLY_PROJECTION",
                type: "decimal(18,6)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "FeedMortalityReductionPercent",
                table: "RII_BUDGET_PLAN_MONTHLY_PROJECTION",
                type: "decimal(18,6)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "GrowthQualityPercent",
                table: "RII_BUDGET_PLAN_MONTHLY_PROJECTION",
                type: "decimal(18,6)",
                nullable: false,
                defaultValue: 100m);

            migrationBuilder.AddColumn<decimal>(
                name: "RawMonthlyGrowthGram",
                table: "RII_BUDGET_PLAN_MONTHLY_PROJECTION",
                type: "decimal(18,6)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MortalityReductionKg",
                table: "RII_BUDGET_PLAN_FEEDING_LINE",
                type: "decimal(18,6)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MortalityReductionPercent",
                table: "RII_BUDGET_PLAN_FEEDING_LINE",
                type: "decimal(18,6)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "RII_BUDGET_FEED_MORTALITY_RATE",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WaterTemperatureId = table.Column<long>(type: "bigint", nullable: false),
                    CalibrationDefinitionId = table.Column<long>(type: "bigint", nullable: false),
                    FeedStockId = table.Column<long>(type: "bigint", nullable: false),
                    ReductionRatePercent = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_RII_BUDGET_FEED_MORTALITY_RATE", x => x.Id);
                    table.CheckConstraint("CK_RII_BUDGET_FEED_MORTALITY_RATE_PERCENT", "[ReductionRatePercent] >= 0 AND [ReductionRatePercent] <= 100");
                    table.ForeignKey(
                        name: "FK_RII_BUDGET_FEED_MORTALITY_RATE_RII_BUDGET_CALIBRATION_DEFINITION_CalibrationDefinitionId",
                        column: x => x.CalibrationDefinitionId,
                        principalTable: "RII_BUDGET_CALIBRATION_DEFINITION",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RII_BUDGET_FEED_MORTALITY_RATE_RII_BUDGET_WATER_TEMPERATURE_WaterTemperatureId",
                        column: x => x.WaterTemperatureId,
                        principalTable: "RII_BUDGET_WATER_TEMPERATURE",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RII_BUDGET_FEED_MORTALITY_RATE_RII_STOCK_FeedStockId",
                        column: x => x.FeedStockId,
                        principalTable: "RII_STOCK",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RII_BUDGET_FEED_MORTALITY_RATE_RII_USERS_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "RII_USERS",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_BUDGET_FEED_MORTALITY_RATE_RII_USERS_DeletedBy",
                        column: x => x.DeletedBy,
                        principalTable: "RII_USERS",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_BUDGET_FEED_MORTALITY_RATE_RII_USERS_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "RII_USERS",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RII_BUDGET_FISH_GROWTH_QUALITY",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FishStockId = table.Column<long>(type: "bigint", nullable: false),
                    GrowthMonthNo = table.Column<int>(type: "int", nullable: false),
                    QualityPercent = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_RII_BUDGET_FISH_GROWTH_QUALITY", x => x.Id);
                    table.CheckConstraint("CK_RII_BUDGET_FISH_GROWTH_QUALITY_MONTH", "[GrowthMonthNo] >= 1 AND [GrowthMonthNo] <= 120");
                    table.CheckConstraint("CK_RII_BUDGET_FISH_GROWTH_QUALITY_PERCENT", "[QualityPercent] >= 0 AND [QualityPercent] <= 100");
                    table.ForeignKey(
                        name: "FK_RII_BUDGET_FISH_GROWTH_QUALITY_RII_STOCK_FishStockId",
                        column: x => x.FishStockId,
                        principalTable: "RII_STOCK",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RII_BUDGET_FISH_GROWTH_QUALITY_RII_USERS_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "RII_USERS",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_BUDGET_FISH_GROWTH_QUALITY_RII_USERS_DeletedBy",
                        column: x => x.DeletedBy,
                        principalTable: "RII_USERS",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_BUDGET_FISH_GROWTH_QUALITY_RII_USERS_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "RII_USERS",
                        principalColumn: "Id");
                });

            migrationBuilder.AddCheckConstraint(
                name: "CK_RII_BUDGET_PLAN_MONTHLY_PROJECTION_ADJUSTMENT_PERCENT",
                table: "RII_BUDGET_PLAN_MONTHLY_PROJECTION",
                sql: "[GrowthQualityPercent] >= 0 AND [GrowthQualityPercent] <= 100 AND [FeedMortalityReductionPercent] >= 0 AND [FeedMortalityReductionPercent] <= 100");

            migrationBuilder.AddCheckConstraint(
                name: "CK_RII_BUDGET_PLAN_FEEDING_LINE_NON_NEGATIVE",
                table: "RII_BUDGET_PLAN_FEEDING_LINE",
                sql: "[FeedAmountRate] >= 0 AND [MortalityReductionPercent] >= 0 AND [MortalityReductionKg] >= 0 AND [FeedKg] >= 0");

            migrationBuilder.CreateIndex(
                name: "IX_RII_BUDGET_FEED_MORTALITY_RATE_CalibrationDefinitionId",
                table: "RII_BUDGET_FEED_MORTALITY_RATE",
                column: "CalibrationDefinitionId");
            migrationBuilder.CreateIndex(name: "IX_RII_BUDGET_FEED_MORTALITY_RATE_CreatedBy", table: "RII_BUDGET_FEED_MORTALITY_RATE", column: "CreatedBy");
            migrationBuilder.CreateIndex(name: "IX_RII_BUDGET_FEED_MORTALITY_RATE_DeletedBy", table: "RII_BUDGET_FEED_MORTALITY_RATE", column: "DeletedBy");
            migrationBuilder.CreateIndex(name: "IX_RII_BUDGET_FEED_MORTALITY_RATE_FeedStockId", table: "RII_BUDGET_FEED_MORTALITY_RATE", column: "FeedStockId");
            migrationBuilder.CreateIndex(name: "IX_RII_BUDGET_FEED_MORTALITY_RATE_UpdatedBy", table: "RII_BUDGET_FEED_MORTALITY_RATE", column: "UpdatedBy");
            migrationBuilder.CreateIndex(
                name: "UX_RII_BUDGET_FEED_MORTALITY_RATE_COMBINATION_ACTIVE",
                table: "RII_BUDGET_FEED_MORTALITY_RATE",
                columns: new[] { "WaterTemperatureId", "CalibrationDefinitionId", "FeedStockId" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(name: "IX_RII_BUDGET_FISH_GROWTH_QUALITY_CreatedBy", table: "RII_BUDGET_FISH_GROWTH_QUALITY", column: "CreatedBy");
            migrationBuilder.CreateIndex(name: "IX_RII_BUDGET_FISH_GROWTH_QUALITY_DeletedBy", table: "RII_BUDGET_FISH_GROWTH_QUALITY", column: "DeletedBy");
            migrationBuilder.CreateIndex(name: "IX_RII_BUDGET_FISH_GROWTH_QUALITY_UpdatedBy", table: "RII_BUDGET_FISH_GROWTH_QUALITY", column: "UpdatedBy");
            migrationBuilder.CreateIndex(
                name: "UX_RII_BUDGET_FISH_GROWTH_QUALITY_STOCK_MONTH_ACTIVE",
                table: "RII_BUDGET_FISH_GROWTH_QUALITY",
                columns: new[] { "FishStockId", "GrowthMonthNo" },
                unique: true,
                filter: "[IsDeleted] = 0");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "RII_BUDGET_FEED_MORTALITY_RATE");
            migrationBuilder.DropTable(name: "RII_BUDGET_FISH_GROWTH_QUALITY");

            migrationBuilder.DropCheckConstraint(
                name: "CK_RII_BUDGET_PLAN_MONTHLY_PROJECTION_ADJUSTMENT_PERCENT",
                table: "RII_BUDGET_PLAN_MONTHLY_PROJECTION");
            migrationBuilder.DropCheckConstraint(
                name: "CK_RII_BUDGET_PLAN_FEEDING_LINE_NON_NEGATIVE",
                table: "RII_BUDGET_PLAN_FEEDING_LINE");

            migrationBuilder.DropColumn(name: "FeedMortalityReductionKg", table: "RII_BUDGET_PLAN_MONTHLY_PROJECTION");
            migrationBuilder.DropColumn(name: "FeedMortalityReductionPercent", table: "RII_BUDGET_PLAN_MONTHLY_PROJECTION");
            migrationBuilder.DropColumn(name: "GrowthQualityPercent", table: "RII_BUDGET_PLAN_MONTHLY_PROJECTION");
            migrationBuilder.DropColumn(name: "RawMonthlyGrowthGram", table: "RII_BUDGET_PLAN_MONTHLY_PROJECTION");
            migrationBuilder.DropColumn(name: "MortalityReductionKg", table: "RII_BUDGET_PLAN_FEEDING_LINE");
            migrationBuilder.DropColumn(name: "MortalityReductionPercent", table: "RII_BUDGET_PLAN_FEEDING_LINE");

            migrationBuilder.AddCheckConstraint(
                name: "CK_RII_BUDGET_PLAN_FEEDING_LINE_NON_NEGATIVE",
                table: "RII_BUDGET_PLAN_FEEDING_LINE",
                sql: "[FeedAmountRate] >= 0 AND [FeedKg] >= 0");
        }
    }
}
