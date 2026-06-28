using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace aqua_api.Migrations
{
    /// <inheritdoc />
    public partial class AddBudgetPlanningModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RII_BUDGET_MortalityRateDefinition",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FishStockId = table.Column<long>(type: "bigint", nullable: true),
                    CalibrationDefinitionId = table.Column<long>(type: "bigint", nullable: true),
                    GrowthMonthNo = table.Column<int>(type: "int", nullable: true),
                    MortalityRatePercent = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
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
                    table.PrimaryKey("PK_RII_BUDGET_MortalityRateDefinition", x => x.Id);
                    table.CheckConstraint("CK_RII_BUDGET_MortalityRateDefinition_Rate", "[MortalityRatePercent] >= 0 AND [MortalityRatePercent] <= 100");
                    table.ForeignKey(
                        name: "FK_RII_BUDGET_MortalityRateDefinition_RII_BUDGET_CALIBRATION_DEFINITION_CalibrationDefinitionId",
                        column: x => x.CalibrationDefinitionId,
                        principalTable: "RII_BUDGET_CALIBRATION_DEFINITION",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_BUDGET_MortalityRateDefinition_RII_STOCK_FishStockId",
                        column: x => x.FishStockId,
                        principalTable: "RII_STOCK",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_BUDGET_MortalityRateDefinition_RII_USERS_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "RII_USERS",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_BUDGET_MortalityRateDefinition_RII_USERS_DeletedBy",
                        column: x => x.DeletedBy,
                        principalTable: "RII_USERS",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_BUDGET_MortalityRateDefinition_RII_USERS_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "RII_USERS",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RII_BUDGET_Plan",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BudgetNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    BudgetCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    BudgetName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    StartYear = table.Column<int>(type: "int", nullable: false),
                    StartMonth = table.Column<int>(type: "int", nullable: false),
                    EndYear = table.Column<int>(type: "int", nullable: false),
                    EndMonth = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CalculatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
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
                    table.PrimaryKey("PK_RII_BUDGET_Plan", x => x.Id);
                    table.CheckConstraint("CK_RII_BUDGET_Plan_EndMonth", "[EndMonth] BETWEEN 1 AND 12");
                    table.CheckConstraint("CK_RII_BUDGET_Plan_StartMonth", "[StartMonth] BETWEEN 1 AND 12");
                    table.CheckConstraint("CK_RII_BUDGET_Plan_Status", "[Status] IN (0,1,2,3)");
                    table.ForeignKey(
                        name: "FK_RII_BUDGET_Plan_RII_USERS_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "RII_USERS",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_BUDGET_Plan_RII_USERS_DeletedBy",
                        column: x => x.DeletedBy,
                        principalTable: "RII_USERS",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_BUDGET_Plan_RII_USERS_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "RII_USERS",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RII_BUDGET_PlanProject",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BudgetPlanId = table.Column<long>(type: "bigint", nullable: false),
                    SourceType = table.Column<byte>(type: "tinyint", nullable: false),
                    SourceProjectId = table.Column<long>(type: "bigint", nullable: true),
                    ProjectCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ProjectName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    StartDate = table.Column<DateTime>(type: "date", nullable: true),
                    EndDate = table.Column<DateTime>(type: "date", nullable: true),
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
                    table.PrimaryKey("PK_RII_BUDGET_PlanProject", x => x.Id);
                    table.CheckConstraint("CK_RII_BUDGET_PlanProject_SourceType", "[SourceType] IN (0,1)");
                    table.ForeignKey(
                        name: "FK_RII_BUDGET_PlanProject_RII_BUDGET_Plan_BudgetPlanId",
                        column: x => x.BudgetPlanId,
                        principalTable: "RII_BUDGET_Plan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RII_BUDGET_PlanProject_RII_Project_SourceProjectId",
                        column: x => x.SourceProjectId,
                        principalTable: "RII_Project",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_BUDGET_PlanProject_RII_USERS_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "RII_USERS",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_BUDGET_PlanProject_RII_USERS_DeletedBy",
                        column: x => x.DeletedBy,
                        principalTable: "RII_USERS",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_BUDGET_PlanProject_RII_USERS_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "RII_USERS",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RII_BUDGET_PlanFishBatch",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BudgetPlanId = table.Column<long>(type: "bigint", nullable: false),
                    BudgetPlanProjectId = table.Column<long>(type: "bigint", nullable: false),
                    SourceType = table.Column<byte>(type: "tinyint", nullable: false),
                    SourceFishBatchId = table.Column<long>(type: "bigint", nullable: true),
                    FishStockId = table.Column<long>(type: "bigint", nullable: false),
                    BatchCode = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    InitialLiveCount = table.Column<int>(type: "int", nullable: false),
                    InitialAverageGram = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    InitialBiomassKg = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    GrowthStartYear = table.Column<int>(type: "int", nullable: false),
                    GrowthStartMonth = table.Column<int>(type: "int", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_RII_BUDGET_PlanFishBatch", x => x.Id);
                    table.CheckConstraint("CK_RII_BUDGET_PlanFishBatch_GrowthStartMonth", "[GrowthStartMonth] BETWEEN 1 AND 12");
                    table.CheckConstraint("CK_RII_BUDGET_PlanFishBatch_NonNegative", "[InitialLiveCount] >= 0 AND [InitialAverageGram] >= 0 AND [InitialBiomassKg] >= 0");
                    table.CheckConstraint("CK_RII_BUDGET_PlanFishBatch_SourceType", "[SourceType] IN (0,1)");
                    table.ForeignKey(
                        name: "FK_RII_BUDGET_PlanFishBatch_RII_BUDGET_PlanProject_BudgetPlanProjectId",
                        column: x => x.BudgetPlanProjectId,
                        principalTable: "RII_BUDGET_PlanProject",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_BUDGET_PlanFishBatch_RII_BUDGET_Plan_BudgetPlanId",
                        column: x => x.BudgetPlanId,
                        principalTable: "RII_BUDGET_Plan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RII_BUDGET_PlanFishBatch_RII_FishBatch_SourceFishBatchId",
                        column: x => x.SourceFishBatchId,
                        principalTable: "RII_FishBatch",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_BUDGET_PlanFishBatch_RII_STOCK_FishStockId",
                        column: x => x.FishStockId,
                        principalTable: "RII_STOCK",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_BUDGET_PlanFishBatch_RII_USERS_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "RII_USERS",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_BUDGET_PlanFishBatch_RII_USERS_DeletedBy",
                        column: x => x.DeletedBy,
                        principalTable: "RII_USERS",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_BUDGET_PlanFishBatch_RII_USERS_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "RII_USERS",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RII_BUDGET_PlanMonthlyProjection",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BudgetPlanId = table.Column<long>(type: "bigint", nullable: false),
                    BudgetPlanFishBatchId = table.Column<long>(type: "bigint", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    Month = table.Column<int>(type: "int", nullable: false),
                    MonthIndex = table.Column<int>(type: "int", nullable: false),
                    OpeningLiveCount = table.Column<int>(type: "int", nullable: false),
                    OpeningAverageGram = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    OpeningBiomassKg = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    MonthlyGrowthGram = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    ClosingAverageGram = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    SalesKg = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    SalesCount = table.Column<int>(type: "int", nullable: false),
                    MortalityKg = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    MortalityCount = table.Column<int>(type: "int", nullable: false),
                    FeedKg = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    ClosingLiveCount = table.Column<int>(type: "int", nullable: false),
                    ClosingBiomassKg = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    CalibrationDefinitionId = table.Column<long>(type: "bigint", nullable: true),
                    WaterTemperatureId = table.Column<long>(type: "bigint", nullable: true),
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
                    table.PrimaryKey("PK_RII_BUDGET_PlanMonthlyProjection", x => x.Id);
                    table.CheckConstraint("CK_RII_BUDGET_PlanMonthlyProjection_Month", "[Month] BETWEEN 1 AND 12");
                    table.CheckConstraint("CK_RII_BUDGET_PlanMonthlyProjection_NonNegative", "[OpeningLiveCount] >= 0 AND [ClosingLiveCount] >= 0 AND [OpeningBiomassKg] >= 0 AND [ClosingBiomassKg] >= 0");
                    table.ForeignKey(
                        name: "FK_RII_BUDGET_PlanMonthlyProjection_RII_BUDGET_CALIBRATION_DEFINITION_CalibrationDefinitionId",
                        column: x => x.CalibrationDefinitionId,
                        principalTable: "RII_BUDGET_CALIBRATION_DEFINITION",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_BUDGET_PlanMonthlyProjection_RII_BUDGET_PlanFishBatch_BudgetPlanFishBatchId",
                        column: x => x.BudgetPlanFishBatchId,
                        principalTable: "RII_BUDGET_PlanFishBatch",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_BUDGET_PlanMonthlyProjection_RII_BUDGET_Plan_BudgetPlanId",
                        column: x => x.BudgetPlanId,
                        principalTable: "RII_BUDGET_Plan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RII_BUDGET_PlanMonthlyProjection_RII_BUDGET_WATER_TEMPERATURE_WaterTemperatureId",
                        column: x => x.WaterTemperatureId,
                        principalTable: "RII_BUDGET_WATER_TEMPERATURE",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_BUDGET_PlanMonthlyProjection_RII_USERS_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "RII_USERS",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_BUDGET_PlanMonthlyProjection_RII_USERS_DeletedBy",
                        column: x => x.DeletedBy,
                        principalTable: "RII_USERS",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_BUDGET_PlanMonthlyProjection_RII_USERS_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "RII_USERS",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RII_BUDGET_PlanSalesLine",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BudgetPlanId = table.Column<long>(type: "bigint", nullable: false),
                    BudgetPlanFishBatchId = table.Column<long>(type: "bigint", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    Month = table.Column<int>(type: "int", nullable: false),
                    SalesKg = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    SalesCount = table.Column<int>(type: "int", nullable: true),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,6)", nullable: true),
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
                    table.PrimaryKey("PK_RII_BUDGET_PlanSalesLine", x => x.Id);
                    table.CheckConstraint("CK_RII_BUDGET_PlanSalesLine_Month", "[Month] BETWEEN 1 AND 12");
                    table.CheckConstraint("CK_RII_BUDGET_PlanSalesLine_NonNegative", "[SalesKg] >= 0");
                    table.ForeignKey(
                        name: "FK_RII_BUDGET_PlanSalesLine_RII_BUDGET_PlanFishBatch_BudgetPlanFishBatchId",
                        column: x => x.BudgetPlanFishBatchId,
                        principalTable: "RII_BUDGET_PlanFishBatch",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_BUDGET_PlanSalesLine_RII_BUDGET_Plan_BudgetPlanId",
                        column: x => x.BudgetPlanId,
                        principalTable: "RII_BUDGET_Plan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RII_BUDGET_PlanSalesLine_RII_USERS_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "RII_USERS",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_BUDGET_PlanSalesLine_RII_USERS_DeletedBy",
                        column: x => x.DeletedBy,
                        principalTable: "RII_USERS",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_BUDGET_PlanSalesLine_RII_USERS_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "RII_USERS",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RII_BUDGET_PlanFeedingLine",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BudgetPlanId = table.Column<long>(type: "bigint", nullable: false),
                    BudgetPlanMonthlyProjectionId = table.Column<long>(type: "bigint", nullable: false),
                    BudgetPlanFishBatchId = table.Column<long>(type: "bigint", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    Month = table.Column<int>(type: "int", nullable: false),
                    FeedStockId = table.Column<long>(type: "bigint", nullable: true),
                    FeedAmountRate = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    FeedKg = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
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
                    table.PrimaryKey("PK_RII_BUDGET_PlanFeedingLine", x => x.Id);
                    table.CheckConstraint("CK_RII_BUDGET_PlanFeedingLine_Month", "[Month] BETWEEN 1 AND 12");
                    table.CheckConstraint("CK_RII_BUDGET_PlanFeedingLine_NonNegative", "[FeedAmountRate] >= 0 AND [FeedKg] >= 0");
                    table.ForeignKey(
                        name: "FK_RII_BUDGET_PlanFeedingLine_RII_BUDGET_PlanFishBatch_BudgetPlanFishBatchId",
                        column: x => x.BudgetPlanFishBatchId,
                        principalTable: "RII_BUDGET_PlanFishBatch",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_BUDGET_PlanFeedingLine_RII_BUDGET_PlanMonthlyProjection_BudgetPlanMonthlyProjectionId",
                        column: x => x.BudgetPlanMonthlyProjectionId,
                        principalTable: "RII_BUDGET_PlanMonthlyProjection",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_BUDGET_PlanFeedingLine_RII_BUDGET_Plan_BudgetPlanId",
                        column: x => x.BudgetPlanId,
                        principalTable: "RII_BUDGET_Plan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RII_BUDGET_PlanFeedingLine_RII_STOCK_FeedStockId",
                        column: x => x.FeedStockId,
                        principalTable: "RII_STOCK",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_BUDGET_PlanFeedingLine_RII_USERS_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "RII_USERS",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_BUDGET_PlanFeedingLine_RII_USERS_DeletedBy",
                        column: x => x.DeletedBy,
                        principalTable: "RII_USERS",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_BUDGET_PlanFeedingLine_RII_USERS_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "RII_USERS",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RII_BUDGET_PlanMortalityLine",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BudgetPlanId = table.Column<long>(type: "bigint", nullable: false),
                    BudgetPlanMonthlyProjectionId = table.Column<long>(type: "bigint", nullable: false),
                    BudgetPlanFishBatchId = table.Column<long>(type: "bigint", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    Month = table.Column<int>(type: "int", nullable: false),
                    MortalityRatePercent = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    MortalityCount = table.Column<int>(type: "int", nullable: false),
                    MortalityKg = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
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
                    table.PrimaryKey("PK_RII_BUDGET_PlanMortalityLine", x => x.Id);
                    table.CheckConstraint("CK_RII_BUDGET_PlanMortalityLine_Month", "[Month] BETWEEN 1 AND 12");
                    table.CheckConstraint("CK_RII_BUDGET_PlanMortalityLine_NonNegative", "[MortalityRatePercent] >= 0 AND [MortalityCount] >= 0 AND [MortalityKg] >= 0");
                    table.ForeignKey(
                        name: "FK_RII_BUDGET_PlanMortalityLine_RII_BUDGET_PlanFishBatch_BudgetPlanFishBatchId",
                        column: x => x.BudgetPlanFishBatchId,
                        principalTable: "RII_BUDGET_PlanFishBatch",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_BUDGET_PlanMortalityLine_RII_BUDGET_PlanMonthlyProjection_BudgetPlanMonthlyProjectionId",
                        column: x => x.BudgetPlanMonthlyProjectionId,
                        principalTable: "RII_BUDGET_PlanMonthlyProjection",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_BUDGET_PlanMortalityLine_RII_BUDGET_Plan_BudgetPlanId",
                        column: x => x.BudgetPlanId,
                        principalTable: "RII_BUDGET_Plan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RII_BUDGET_PlanMortalityLine_RII_USERS_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "RII_USERS",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_BUDGET_PlanMortalityLine_RII_USERS_DeletedBy",
                        column: x => x.DeletedBy,
                        principalTable: "RII_USERS",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_BUDGET_PlanMortalityLine_RII_USERS_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "RII_USERS",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_RII_BUDGET_MortalityRateDefinition_CalibrationDefinitionId",
                table: "RII_BUDGET_MortalityRateDefinition",
                column: "CalibrationDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_RII_BUDGET_MortalityRateDefinition_CreatedBy",
                table: "RII_BUDGET_MortalityRateDefinition",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_RII_BUDGET_MortalityRateDefinition_DeletedBy",
                table: "RII_BUDGET_MortalityRateDefinition",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_RII_BUDGET_MortalityRateDefinition_Key_Active",
                table: "RII_BUDGET_MortalityRateDefinition",
                columns: new[] { "FishStockId", "CalibrationDefinitionId", "GrowthMonthNo" },
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_RII_BUDGET_MortalityRateDefinition_UpdatedBy",
                table: "RII_BUDGET_MortalityRateDefinition",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_RII_BUDGET_Plan_CreatedBy",
                table: "RII_BUDGET_Plan",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_RII_BUDGET_Plan_DeletedBy",
                table: "RII_BUDGET_Plan",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_RII_BUDGET_Plan_UpdatedBy",
                table: "RII_BUDGET_Plan",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "UX_RII_BUDGET_Plan_BudgetCode_Active",
                table: "RII_BUDGET_Plan",
                column: "BudgetCode",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "UX_RII_BUDGET_Plan_BudgetNo_Active",
                table: "RII_BUDGET_Plan",
                column: "BudgetNo",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_RII_BUDGET_PlanFeedingLine_BudgetPlanFishBatchId",
                table: "RII_BUDGET_PlanFeedingLine",
                column: "BudgetPlanFishBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_RII_BUDGET_PlanFeedingLine_BudgetPlanId",
                table: "RII_BUDGET_PlanFeedingLine",
                column: "BudgetPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_RII_BUDGET_PlanFeedingLine_BudgetPlanMonthlyProjectionId",
                table: "RII_BUDGET_PlanFeedingLine",
                column: "BudgetPlanMonthlyProjectionId");

            migrationBuilder.CreateIndex(
                name: "IX_RII_BUDGET_PlanFeedingLine_CreatedBy",
                table: "RII_BUDGET_PlanFeedingLine",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_RII_BUDGET_PlanFeedingLine_DeletedBy",
                table: "RII_BUDGET_PlanFeedingLine",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_RII_BUDGET_PlanFeedingLine_FeedStockId",
                table: "RII_BUDGET_PlanFeedingLine",
                column: "FeedStockId");

            migrationBuilder.CreateIndex(
                name: "IX_RII_BUDGET_PlanFeedingLine_UpdatedBy",
                table: "RII_BUDGET_PlanFeedingLine",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_RII_BUDGET_PlanFishBatch_BatchCode_Active",
                table: "RII_BUDGET_PlanFishBatch",
                columns: new[] { "BudgetPlanId", "BatchCode" },
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_RII_BUDGET_PlanFishBatch_BudgetPlanProjectId",
                table: "RII_BUDGET_PlanFishBatch",
                column: "BudgetPlanProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_RII_BUDGET_PlanFishBatch_CreatedBy",
                table: "RII_BUDGET_PlanFishBatch",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_RII_BUDGET_PlanFishBatch_DeletedBy",
                table: "RII_BUDGET_PlanFishBatch",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_RII_BUDGET_PlanFishBatch_FishStockId",
                table: "RII_BUDGET_PlanFishBatch",
                column: "FishStockId");

            migrationBuilder.CreateIndex(
                name: "IX_RII_BUDGET_PlanFishBatch_SourceFishBatchId",
                table: "RII_BUDGET_PlanFishBatch",
                column: "SourceFishBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_RII_BUDGET_PlanFishBatch_UpdatedBy",
                table: "RII_BUDGET_PlanFishBatch",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_RII_BUDGET_PlanMonthlyProjection_BudgetPlanFishBatchId",
                table: "RII_BUDGET_PlanMonthlyProjection",
                column: "BudgetPlanFishBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_RII_BUDGET_PlanMonthlyProjection_CalibrationDefinitionId",
                table: "RII_BUDGET_PlanMonthlyProjection",
                column: "CalibrationDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_RII_BUDGET_PlanMonthlyProjection_CreatedBy",
                table: "RII_BUDGET_PlanMonthlyProjection",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_RII_BUDGET_PlanMonthlyProjection_DeletedBy",
                table: "RII_BUDGET_PlanMonthlyProjection",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_RII_BUDGET_PlanMonthlyProjection_UpdatedBy",
                table: "RII_BUDGET_PlanMonthlyProjection",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_RII_BUDGET_PlanMonthlyProjection_WaterTemperatureId",
                table: "RII_BUDGET_PlanMonthlyProjection",
                column: "WaterTemperatureId");

            migrationBuilder.CreateIndex(
                name: "UX_RII_BUDGET_PlanMonthlyProjection_Period_Active",
                table: "RII_BUDGET_PlanMonthlyProjection",
                columns: new[] { "BudgetPlanId", "BudgetPlanFishBatchId", "Year", "Month" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_RII_BUDGET_PlanMortalityLine_BudgetPlanFishBatchId",
                table: "RII_BUDGET_PlanMortalityLine",
                column: "BudgetPlanFishBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_RII_BUDGET_PlanMortalityLine_BudgetPlanId",
                table: "RII_BUDGET_PlanMortalityLine",
                column: "BudgetPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_RII_BUDGET_PlanMortalityLine_BudgetPlanMonthlyProjectionId",
                table: "RII_BUDGET_PlanMortalityLine",
                column: "BudgetPlanMonthlyProjectionId");

            migrationBuilder.CreateIndex(
                name: "IX_RII_BUDGET_PlanMortalityLine_CreatedBy",
                table: "RII_BUDGET_PlanMortalityLine",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_RII_BUDGET_PlanMortalityLine_DeletedBy",
                table: "RII_BUDGET_PlanMortalityLine",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_RII_BUDGET_PlanMortalityLine_UpdatedBy",
                table: "RII_BUDGET_PlanMortalityLine",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_RII_BUDGET_PlanProject_CreatedBy",
                table: "RII_BUDGET_PlanProject",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_RII_BUDGET_PlanProject_DeletedBy",
                table: "RII_BUDGET_PlanProject",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_RII_BUDGET_PlanProject_ProjectCode_Active",
                table: "RII_BUDGET_PlanProject",
                columns: new[] { "BudgetPlanId", "ProjectCode" },
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_RII_BUDGET_PlanProject_SourceProjectId",
                table: "RII_BUDGET_PlanProject",
                column: "SourceProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_RII_BUDGET_PlanProject_UpdatedBy",
                table: "RII_BUDGET_PlanProject",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_RII_BUDGET_PlanSalesLine_BudgetPlanFishBatchId",
                table: "RII_BUDGET_PlanSalesLine",
                column: "BudgetPlanFishBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_RII_BUDGET_PlanSalesLine_CreatedBy",
                table: "RII_BUDGET_PlanSalesLine",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_RII_BUDGET_PlanSalesLine_DeletedBy",
                table: "RII_BUDGET_PlanSalesLine",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_RII_BUDGET_PlanSalesLine_Period_Active",
                table: "RII_BUDGET_PlanSalesLine",
                columns: new[] { "BudgetPlanId", "BudgetPlanFishBatchId", "Year", "Month" },
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_RII_BUDGET_PlanSalesLine_UpdatedBy",
                table: "RII_BUDGET_PlanSalesLine",
                column: "UpdatedBy");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RII_BUDGET_MortalityRateDefinition");

            migrationBuilder.DropTable(
                name: "RII_BUDGET_PlanFeedingLine");

            migrationBuilder.DropTable(
                name: "RII_BUDGET_PlanMortalityLine");

            migrationBuilder.DropTable(
                name: "RII_BUDGET_PlanSalesLine");

            migrationBuilder.DropTable(
                name: "RII_BUDGET_PlanMonthlyProjection");

            migrationBuilder.DropTable(
                name: "RII_BUDGET_PlanFishBatch");

            migrationBuilder.DropTable(
                name: "RII_BUDGET_PlanProject");

            migrationBuilder.DropTable(
                name: "RII_BUDGET_Plan");
        }
    }
}
