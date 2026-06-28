using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace aqua_api.Migrations
{
    /// <inheritdoc />
    public partial class RenameBudgetPlanningTablesToUppercase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RII_BUDGET_MortalityRateDefinition_RII_BUDGET_CALIBRATION_DEFINITION_CalibrationDefinitionId",
                table: "RII_BUDGET_MortalityRateDefinition");

            migrationBuilder.DropForeignKey(
                name: "FK_RII_BUDGET_MortalityRateDefinition_RII_STOCK_FishStockId",
                table: "RII_BUDGET_MortalityRateDefinition");

            migrationBuilder.DropForeignKey(
                name: "FK_RII_BUDGET_MortalityRateDefinition_RII_USERS_CreatedBy",
                table: "RII_BUDGET_MortalityRateDefinition");

            migrationBuilder.DropForeignKey(
                name: "FK_RII_BUDGET_MortalityRateDefinition_RII_USERS_DeletedBy",
                table: "RII_BUDGET_MortalityRateDefinition");

            migrationBuilder.DropForeignKey(
                name: "FK_RII_BUDGET_MortalityRateDefinition_RII_USERS_UpdatedBy",
                table: "RII_BUDGET_MortalityRateDefinition");

            migrationBuilder.DropForeignKey(
                name: "FK_RII_BUDGET_Plan_RII_USERS_CreatedBy",
                table: "RII_BUDGET_Plan");

            migrationBuilder.DropForeignKey(
                name: "FK_RII_BUDGET_Plan_RII_USERS_DeletedBy",
                table: "RII_BUDGET_Plan");

            migrationBuilder.DropForeignKey(
                name: "FK_RII_BUDGET_Plan_RII_USERS_UpdatedBy",
                table: "RII_BUDGET_Plan");

            migrationBuilder.DropForeignKey(
                name: "FK_RII_BUDGET_PlanExchangeRate_RII_BUDGET_Plan_BudgetPlanId",
                table: "RII_BUDGET_PlanExchangeRate");

            migrationBuilder.DropForeignKey(
                name: "FK_RII_BUDGET_PlanExchangeRate_RII_USERS_CreatedBy",
                table: "RII_BUDGET_PlanExchangeRate");

            migrationBuilder.DropForeignKey(
                name: "FK_RII_BUDGET_PlanExchangeRate_RII_USERS_DeletedBy",
                table: "RII_BUDGET_PlanExchangeRate");

            migrationBuilder.DropForeignKey(
                name: "FK_RII_BUDGET_PlanExchangeRate_RII_USERS_UpdatedBy",
                table: "RII_BUDGET_PlanExchangeRate");

            migrationBuilder.DropForeignKey(
                name: "FK_RII_BUDGET_PlanFeedingLine_RII_BUDGET_PlanFishBatch_BudgetPlanFishBatchId",
                table: "RII_BUDGET_PlanFeedingLine");

            migrationBuilder.DropForeignKey(
                name: "FK_RII_BUDGET_PlanFeedingLine_RII_BUDGET_PlanMonthlyProjection_BudgetPlanMonthlyProjectionId",
                table: "RII_BUDGET_PlanFeedingLine");

            migrationBuilder.DropForeignKey(
                name: "FK_RII_BUDGET_PlanFeedingLine_RII_BUDGET_Plan_BudgetPlanId",
                table: "RII_BUDGET_PlanFeedingLine");

            migrationBuilder.DropForeignKey(
                name: "FK_RII_BUDGET_PlanFeedingLine_RII_STOCK_FeedStockId",
                table: "RII_BUDGET_PlanFeedingLine");

            migrationBuilder.DropForeignKey(
                name: "FK_RII_BUDGET_PlanFeedingLine_RII_USERS_CreatedBy",
                table: "RII_BUDGET_PlanFeedingLine");

            migrationBuilder.DropForeignKey(
                name: "FK_RII_BUDGET_PlanFeedingLine_RII_USERS_DeletedBy",
                table: "RII_BUDGET_PlanFeedingLine");

            migrationBuilder.DropForeignKey(
                name: "FK_RII_BUDGET_PlanFeedingLine_RII_USERS_UpdatedBy",
                table: "RII_BUDGET_PlanFeedingLine");

            migrationBuilder.DropForeignKey(
                name: "FK_RII_BUDGET_PlanFishBatch_RII_BUDGET_PlanProject_BudgetPlanProjectId",
                table: "RII_BUDGET_PlanFishBatch");

            migrationBuilder.DropForeignKey(
                name: "FK_RII_BUDGET_PlanFishBatch_RII_BUDGET_Plan_BudgetPlanId",
                table: "RII_BUDGET_PlanFishBatch");

            migrationBuilder.DropForeignKey(
                name: "FK_RII_BUDGET_PlanFishBatch_RII_FishBatch_SourceFishBatchId",
                table: "RII_BUDGET_PlanFishBatch");

            migrationBuilder.DropForeignKey(
                name: "FK_RII_BUDGET_PlanFishBatch_RII_STOCK_FishStockId",
                table: "RII_BUDGET_PlanFishBatch");

            migrationBuilder.DropForeignKey(
                name: "FK_RII_BUDGET_PlanFishBatch_RII_USERS_CreatedBy",
                table: "RII_BUDGET_PlanFishBatch");

            migrationBuilder.DropForeignKey(
                name: "FK_RII_BUDGET_PlanFishBatch_RII_USERS_DeletedBy",
                table: "RII_BUDGET_PlanFishBatch");

            migrationBuilder.DropForeignKey(
                name: "FK_RII_BUDGET_PlanFishBatch_RII_USERS_UpdatedBy",
                table: "RII_BUDGET_PlanFishBatch");

            migrationBuilder.DropForeignKey(
                name: "FK_RII_BUDGET_PlanFishBatchAdjustment_RII_BUDGET_PlanFishBatch_BudgetPlanFishBatchId",
                table: "RII_BUDGET_PlanFishBatchAdjustment");

            migrationBuilder.DropForeignKey(
                name: "FK_RII_BUDGET_PlanFishBatchAdjustment_RII_BUDGET_Plan_BudgetPlanId",
                table: "RII_BUDGET_PlanFishBatchAdjustment");

            migrationBuilder.DropForeignKey(
                name: "FK_RII_BUDGET_PlanFishBatchAdjustment_RII_USERS_CreatedBy",
                table: "RII_BUDGET_PlanFishBatchAdjustment");

            migrationBuilder.DropForeignKey(
                name: "FK_RII_BUDGET_PlanFishBatchAdjustment_RII_USERS_DeletedBy",
                table: "RII_BUDGET_PlanFishBatchAdjustment");

            migrationBuilder.DropForeignKey(
                name: "FK_RII_BUDGET_PlanFishBatchAdjustment_RII_USERS_UpdatedBy",
                table: "RII_BUDGET_PlanFishBatchAdjustment");

            migrationBuilder.DropForeignKey(
                name: "FK_RII_BUDGET_PlanMonthlyProjection_RII_BUDGET_CALIBRATION_DEFINITION_CalibrationDefinitionId",
                table: "RII_BUDGET_PlanMonthlyProjection");

            migrationBuilder.DropForeignKey(
                name: "FK_RII_BUDGET_PlanMonthlyProjection_RII_BUDGET_PlanFishBatch_BudgetPlanFishBatchId",
                table: "RII_BUDGET_PlanMonthlyProjection");

            migrationBuilder.DropForeignKey(
                name: "FK_RII_BUDGET_PlanMonthlyProjection_RII_BUDGET_Plan_BudgetPlanId",
                table: "RII_BUDGET_PlanMonthlyProjection");

            migrationBuilder.DropForeignKey(
                name: "FK_RII_BUDGET_PlanMonthlyProjection_RII_BUDGET_WATER_TEMPERATURE_WaterTemperatureId",
                table: "RII_BUDGET_PlanMonthlyProjection");

            migrationBuilder.DropForeignKey(
                name: "FK_RII_BUDGET_PlanMonthlyProjection_RII_USERS_CreatedBy",
                table: "RII_BUDGET_PlanMonthlyProjection");

            migrationBuilder.DropForeignKey(
                name: "FK_RII_BUDGET_PlanMonthlyProjection_RII_USERS_DeletedBy",
                table: "RII_BUDGET_PlanMonthlyProjection");

            migrationBuilder.DropForeignKey(
                name: "FK_RII_BUDGET_PlanMonthlyProjection_RII_USERS_UpdatedBy",
                table: "RII_BUDGET_PlanMonthlyProjection");

            migrationBuilder.DropForeignKey(
                name: "FK_RII_BUDGET_PlanMortalityLine_RII_BUDGET_PlanFishBatch_BudgetPlanFishBatchId",
                table: "RII_BUDGET_PlanMortalityLine");

            migrationBuilder.DropForeignKey(
                name: "FK_RII_BUDGET_PlanMortalityLine_RII_BUDGET_PlanMonthlyProjection_BudgetPlanMonthlyProjectionId",
                table: "RII_BUDGET_PlanMortalityLine");

            migrationBuilder.DropForeignKey(
                name: "FK_RII_BUDGET_PlanMortalityLine_RII_BUDGET_Plan_BudgetPlanId",
                table: "RII_BUDGET_PlanMortalityLine");

            migrationBuilder.DropForeignKey(
                name: "FK_RII_BUDGET_PlanMortalityLine_RII_USERS_CreatedBy",
                table: "RII_BUDGET_PlanMortalityLine");

            migrationBuilder.DropForeignKey(
                name: "FK_RII_BUDGET_PlanMortalityLine_RII_USERS_DeletedBy",
                table: "RII_BUDGET_PlanMortalityLine");

            migrationBuilder.DropForeignKey(
                name: "FK_RII_BUDGET_PlanMortalityLine_RII_USERS_UpdatedBy",
                table: "RII_BUDGET_PlanMortalityLine");

            migrationBuilder.DropForeignKey(
                name: "FK_RII_BUDGET_PlanProject_RII_BUDGET_Plan_BudgetPlanId",
                table: "RII_BUDGET_PlanProject");

            migrationBuilder.DropForeignKey(
                name: "FK_RII_BUDGET_PlanProject_RII_Project_SourceProjectId",
                table: "RII_BUDGET_PlanProject");

            migrationBuilder.DropForeignKey(
                name: "FK_RII_BUDGET_PlanProject_RII_USERS_CreatedBy",
                table: "RII_BUDGET_PlanProject");

            migrationBuilder.DropForeignKey(
                name: "FK_RII_BUDGET_PlanProject_RII_USERS_DeletedBy",
                table: "RII_BUDGET_PlanProject");

            migrationBuilder.DropForeignKey(
                name: "FK_RII_BUDGET_PlanProject_RII_USERS_UpdatedBy",
                table: "RII_BUDGET_PlanProject");

            migrationBuilder.DropForeignKey(
                name: "FK_RII_BUDGET_PlanSalesLine_RII_BUDGET_PlanFishBatch_BudgetPlanFishBatchId",
                table: "RII_BUDGET_PlanSalesLine");

            migrationBuilder.DropForeignKey(
                name: "FK_RII_BUDGET_PlanSalesLine_RII_BUDGET_Plan_BudgetPlanId",
                table: "RII_BUDGET_PlanSalesLine");

            migrationBuilder.DropForeignKey(
                name: "FK_RII_BUDGET_PlanSalesLine_RII_USERS_CreatedBy",
                table: "RII_BUDGET_PlanSalesLine");

            migrationBuilder.DropForeignKey(
                name: "FK_RII_BUDGET_PlanSalesLine_RII_USERS_DeletedBy",
                table: "RII_BUDGET_PlanSalesLine");

            migrationBuilder.DropForeignKey(
                name: "FK_RII_BUDGET_PlanSalesLine_RII_USERS_UpdatedBy",
                table: "RII_BUDGET_PlanSalesLine");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RII_BUDGET_Plan",
                table: "RII_BUDGET_Plan");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RII_BUDGET_PlanSalesLine",
                table: "RII_BUDGET_PlanSalesLine");

            migrationBuilder.DropCheckConstraint(
                name: "CK_RII_BUDGET_PlanSalesLine_Month",
                table: "RII_BUDGET_PlanSalesLine");

            migrationBuilder.DropCheckConstraint(
                name: "CK_RII_BUDGET_PlanSalesLine_NonNegative",
                table: "RII_BUDGET_PlanSalesLine");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RII_BUDGET_PlanProject",
                table: "RII_BUDGET_PlanProject");

            migrationBuilder.DropCheckConstraint(
                name: "CK_RII_BUDGET_PlanProject_SourceType",
                table: "RII_BUDGET_PlanProject");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RII_BUDGET_PlanMortalityLine",
                table: "RII_BUDGET_PlanMortalityLine");

            migrationBuilder.DropCheckConstraint(
                name: "CK_RII_BUDGET_PlanMortalityLine_Month",
                table: "RII_BUDGET_PlanMortalityLine");

            migrationBuilder.DropCheckConstraint(
                name: "CK_RII_BUDGET_PlanMortalityLine_NonNegative",
                table: "RII_BUDGET_PlanMortalityLine");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RII_BUDGET_PlanMonthlyProjection",
                table: "RII_BUDGET_PlanMonthlyProjection");

            migrationBuilder.DropCheckConstraint(
                name: "CK_RII_BUDGET_PlanMonthlyProjection_Month",
                table: "RII_BUDGET_PlanMonthlyProjection");

            migrationBuilder.DropCheckConstraint(
                name: "CK_RII_BUDGET_PlanMonthlyProjection_NonNegative",
                table: "RII_BUDGET_PlanMonthlyProjection");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RII_BUDGET_PlanFishBatchAdjustment",
                table: "RII_BUDGET_PlanFishBatchAdjustment");

            migrationBuilder.DropCheckConstraint(
                name: "CK_RII_BUDGET_PlanFishBatchAdjustment_Positive",
                table: "RII_BUDGET_PlanFishBatchAdjustment");

            migrationBuilder.DropCheckConstraint(
                name: "CK_RII_BUDGET_PlanFishBatchAdjustment_Type",
                table: "RII_BUDGET_PlanFishBatchAdjustment");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RII_BUDGET_PlanFishBatch",
                table: "RII_BUDGET_PlanFishBatch");

            migrationBuilder.DropCheckConstraint(
                name: "CK_RII_BUDGET_PlanFishBatch_GrowthStartMonth",
                table: "RII_BUDGET_PlanFishBatch");

            migrationBuilder.DropCheckConstraint(
                name: "CK_RII_BUDGET_PlanFishBatch_NonNegative",
                table: "RII_BUDGET_PlanFishBatch");

            migrationBuilder.DropCheckConstraint(
                name: "CK_RII_BUDGET_PlanFishBatch_SourceType",
                table: "RII_BUDGET_PlanFishBatch");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RII_BUDGET_PlanFeedingLine",
                table: "RII_BUDGET_PlanFeedingLine");

            migrationBuilder.DropCheckConstraint(
                name: "CK_RII_BUDGET_PlanFeedingLine_Month",
                table: "RII_BUDGET_PlanFeedingLine");

            migrationBuilder.DropCheckConstraint(
                name: "CK_RII_BUDGET_PlanFeedingLine_NonNegative",
                table: "RII_BUDGET_PlanFeedingLine");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RII_BUDGET_PlanExchangeRate",
                table: "RII_BUDGET_PlanExchangeRate");

            migrationBuilder.DropCheckConstraint(
                name: "CK_RII_BUDGET_PlanExchangeRate_Month",
                table: "RII_BUDGET_PlanExchangeRate");

            migrationBuilder.DropCheckConstraint(
                name: "CK_RII_BUDGET_PlanExchangeRate_Rate",
                table: "RII_BUDGET_PlanExchangeRate");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RII_BUDGET_MortalityRateDefinition",
                table: "RII_BUDGET_MortalityRateDefinition");

            migrationBuilder.DropCheckConstraint(
                name: "CK_RII_BUDGET_MortalityRateDefinition_Rate",
                table: "RII_BUDGET_MortalityRateDefinition");

            migrationBuilder.RenameTable(
                name: "RII_BUDGET_Plan",
                newName: "RII_BUDGET_PLAN");

            migrationBuilder.RenameTable(
                name: "RII_BUDGET_PlanSalesLine",
                newName: "RII_BUDGET_PLAN_SALES_LINE");

            migrationBuilder.RenameTable(
                name: "RII_BUDGET_PlanProject",
                newName: "RII_BUDGET_PLAN_PROJECT");

            migrationBuilder.RenameTable(
                name: "RII_BUDGET_PlanMortalityLine",
                newName: "RII_BUDGET_PLAN_MORTALITY_LINE");

            migrationBuilder.RenameTable(
                name: "RII_BUDGET_PlanMonthlyProjection",
                newName: "RII_BUDGET_PLAN_MONTHLY_PROJECTION");

            migrationBuilder.RenameTable(
                name: "RII_BUDGET_PlanFishBatchAdjustment",
                newName: "RII_BUDGET_PLAN_FISH_BATCH_ADJUSTMENT");

            migrationBuilder.RenameTable(
                name: "RII_BUDGET_PlanFishBatch",
                newName: "RII_BUDGET_PLAN_FISH_BATCH");

            migrationBuilder.RenameTable(
                name: "RII_BUDGET_PlanFeedingLine",
                newName: "RII_BUDGET_PLAN_FEEDING_LINE");

            migrationBuilder.RenameTable(
                name: "RII_BUDGET_PlanExchangeRate",
                newName: "RII_BUDGET_PLAN_EXCHANGE_RATE");

            migrationBuilder.RenameTable(
                name: "RII_BUDGET_MortalityRateDefinition",
                newName: "RII_BUDGET_MORTALITY_RATE_DEFINITION");

            migrationBuilder.RenameIndex(
                name: "UX_RII_BUDGET_Plan_BudgetNo_Active",
                table: "RII_BUDGET_PLAN",
                newName: "UX_RII_BUDGET_PLAN_BudgetNo_Active");

            migrationBuilder.RenameIndex(
                name: "UX_RII_BUDGET_Plan_BudgetCode_Active",
                table: "RII_BUDGET_PLAN",
                newName: "UX_RII_BUDGET_PLAN_BudgetCode_Active");

            migrationBuilder.RenameIndex(
                name: "IX_RII_BUDGET_Plan_UpdatedBy",
                table: "RII_BUDGET_PLAN",
                newName: "IX_RII_BUDGET_PLAN_UpdatedBy");

            migrationBuilder.RenameIndex(
                name: "IX_RII_BUDGET_Plan_DeletedBy",
                table: "RII_BUDGET_PLAN",
                newName: "IX_RII_BUDGET_PLAN_DeletedBy");

            migrationBuilder.RenameIndex(
                name: "IX_RII_BUDGET_Plan_CreatedBy",
                table: "RII_BUDGET_PLAN",
                newName: "IX_RII_BUDGET_PLAN_CreatedBy");

            migrationBuilder.RenameIndex(
                name: "IX_RII_BUDGET_PlanSalesLine_UpdatedBy",
                table: "RII_BUDGET_PLAN_SALES_LINE",
                newName: "IX_RII_BUDGET_PLAN_SALES_LINE_UpdatedBy");

            migrationBuilder.RenameIndex(
                name: "IX_RII_BUDGET_PlanSalesLine_Period_Active",
                table: "RII_BUDGET_PLAN_SALES_LINE",
                newName: "IX_RII_BUDGET_PLAN_SALES_LINE_Period_Active");

            migrationBuilder.RenameIndex(
                name: "IX_RII_BUDGET_PlanSalesLine_DeletedBy",
                table: "RII_BUDGET_PLAN_SALES_LINE",
                newName: "IX_RII_BUDGET_PLAN_SALES_LINE_DeletedBy");

            migrationBuilder.RenameIndex(
                name: "IX_RII_BUDGET_PlanSalesLine_CreatedBy",
                table: "RII_BUDGET_PLAN_SALES_LINE",
                newName: "IX_RII_BUDGET_PLAN_SALES_LINE_CreatedBy");

            migrationBuilder.RenameIndex(
                name: "IX_RII_BUDGET_PlanSalesLine_BudgetPlanFishBatchId",
                table: "RII_BUDGET_PLAN_SALES_LINE",
                newName: "IX_RII_BUDGET_PLAN_SALES_LINE_BudgetPlanFishBatchId");

            migrationBuilder.RenameIndex(
                name: "IX_RII_BUDGET_PlanProject_UpdatedBy",
                table: "RII_BUDGET_PLAN_PROJECT",
                newName: "IX_RII_BUDGET_PLAN_PROJECT_UpdatedBy");

            migrationBuilder.RenameIndex(
                name: "IX_RII_BUDGET_PlanProject_SourceProjectId",
                table: "RII_BUDGET_PLAN_PROJECT",
                newName: "IX_RII_BUDGET_PLAN_PROJECT_SourceProjectId");

            migrationBuilder.RenameIndex(
                name: "IX_RII_BUDGET_PlanProject_ProjectCode_Active",
                table: "RII_BUDGET_PLAN_PROJECT",
                newName: "IX_RII_BUDGET_PLAN_PROJECT_ProjectCode_Active");

            migrationBuilder.RenameIndex(
                name: "IX_RII_BUDGET_PlanProject_DeletedBy",
                table: "RII_BUDGET_PLAN_PROJECT",
                newName: "IX_RII_BUDGET_PLAN_PROJECT_DeletedBy");

            migrationBuilder.RenameIndex(
                name: "IX_RII_BUDGET_PlanProject_CreatedBy",
                table: "RII_BUDGET_PLAN_PROJECT",
                newName: "IX_RII_BUDGET_PLAN_PROJECT_CreatedBy");

            migrationBuilder.RenameIndex(
                name: "IX_RII_BUDGET_PlanMortalityLine_UpdatedBy",
                table: "RII_BUDGET_PLAN_MORTALITY_LINE",
                newName: "IX_RII_BUDGET_PLAN_MORTALITY_LINE_UpdatedBy");

            migrationBuilder.RenameIndex(
                name: "IX_RII_BUDGET_PlanMortalityLine_DeletedBy",
                table: "RII_BUDGET_PLAN_MORTALITY_LINE",
                newName: "IX_RII_BUDGET_PLAN_MORTALITY_LINE_DeletedBy");

            migrationBuilder.RenameIndex(
                name: "IX_RII_BUDGET_PlanMortalityLine_CreatedBy",
                table: "RII_BUDGET_PLAN_MORTALITY_LINE",
                newName: "IX_RII_BUDGET_PLAN_MORTALITY_LINE_CreatedBy");

            migrationBuilder.RenameIndex(
                name: "IX_RII_BUDGET_PlanMortalityLine_BudgetPlanMonthlyProjectionId",
                table: "RII_BUDGET_PLAN_MORTALITY_LINE",
                newName: "IX_RII_BUDGET_PLAN_MORTALITY_LINE_BudgetPlanMonthlyProjectionId");

            migrationBuilder.RenameIndex(
                name: "IX_RII_BUDGET_PlanMortalityLine_BudgetPlanId",
                table: "RII_BUDGET_PLAN_MORTALITY_LINE",
                newName: "IX_RII_BUDGET_PLAN_MORTALITY_LINE_BudgetPlanId");

            migrationBuilder.RenameIndex(
                name: "IX_RII_BUDGET_PlanMortalityLine_BudgetPlanFishBatchId",
                table: "RII_BUDGET_PLAN_MORTALITY_LINE",
                newName: "IX_RII_BUDGET_PLAN_MORTALITY_LINE_BudgetPlanFishBatchId");

            migrationBuilder.RenameIndex(
                name: "UX_RII_BUDGET_PlanMonthlyProjection_Period_Active",
                table: "RII_BUDGET_PLAN_MONTHLY_PROJECTION",
                newName: "UX_RII_BUDGET_PLAN_MONTHLY_PROJECTION_Period_Active");

            migrationBuilder.RenameIndex(
                name: "IX_RII_BUDGET_PlanMonthlyProjection_WaterTemperatureId",
                table: "RII_BUDGET_PLAN_MONTHLY_PROJECTION",
                newName: "IX_RII_BUDGET_PLAN_MONTHLY_PROJECTION_WaterTemperatureId");

            migrationBuilder.RenameIndex(
                name: "IX_RII_BUDGET_PlanMonthlyProjection_UpdatedBy",
                table: "RII_BUDGET_PLAN_MONTHLY_PROJECTION",
                newName: "IX_RII_BUDGET_PLAN_MONTHLY_PROJECTION_UpdatedBy");

            migrationBuilder.RenameIndex(
                name: "IX_RII_BUDGET_PlanMonthlyProjection_DeletedBy",
                table: "RII_BUDGET_PLAN_MONTHLY_PROJECTION",
                newName: "IX_RII_BUDGET_PLAN_MONTHLY_PROJECTION_DeletedBy");

            migrationBuilder.RenameIndex(
                name: "IX_RII_BUDGET_PlanMonthlyProjection_CreatedBy",
                table: "RII_BUDGET_PLAN_MONTHLY_PROJECTION",
                newName: "IX_RII_BUDGET_PLAN_MONTHLY_PROJECTION_CreatedBy");

            migrationBuilder.RenameIndex(
                name: "IX_RII_BUDGET_PlanMonthlyProjection_CalibrationDefinitionId",
                table: "RII_BUDGET_PLAN_MONTHLY_PROJECTION",
                newName: "IX_RII_BUDGET_PLAN_MONTHLY_PROJECTION_CalibrationDefinitionId");

            migrationBuilder.RenameIndex(
                name: "IX_RII_BUDGET_PlanMonthlyProjection_BudgetPlanFishBatchId",
                table: "RII_BUDGET_PLAN_MONTHLY_PROJECTION",
                newName: "IX_RII_BUDGET_PLAN_MONTHLY_PROJECTION_BudgetPlanFishBatchId");

            migrationBuilder.RenameIndex(
                name: "IX_RII_BUDGET_PlanFishBatchAdjustment_UpdatedBy",
                table: "RII_BUDGET_PLAN_FISH_BATCH_ADJUSTMENT",
                newName: "IX_RII_BUDGET_PLAN_FISH_BATCH_ADJUSTMENT_UpdatedBy");

            migrationBuilder.RenameIndex(
                name: "IX_RII_BUDGET_PlanFishBatchAdjustment_DeletedBy",
                table: "RII_BUDGET_PLAN_FISH_BATCH_ADJUSTMENT",
                newName: "IX_RII_BUDGET_PLAN_FISH_BATCH_ADJUSTMENT_DeletedBy");

            migrationBuilder.RenameIndex(
                name: "IX_RII_BUDGET_PlanFishBatchAdjustment_CreatedBy",
                table: "RII_BUDGET_PLAN_FISH_BATCH_ADJUSTMENT",
                newName: "IX_RII_BUDGET_PLAN_FISH_BATCH_ADJUSTMENT_CreatedBy");

            migrationBuilder.RenameIndex(
                name: "IX_RII_BUDGET_PlanFishBatchAdjustment_BudgetPlanFishBatchId",
                table: "RII_BUDGET_PLAN_FISH_BATCH_ADJUSTMENT",
                newName: "IX_RII_BUDGET_PLAN_FISH_BATCH_ADJUSTMENT_BudgetPlanFishBatchId");

            migrationBuilder.RenameIndex(
                name: "IX_RII_BUDGET_PlanFishBatchAdjustment_Batch",
                table: "RII_BUDGET_PLAN_FISH_BATCH_ADJUSTMENT",
                newName: "IX_RII_BUDGET_PLAN_FISH_BATCH_ADJUSTMENT_Batch");

            migrationBuilder.RenameIndex(
                name: "IX_RII_BUDGET_PlanFishBatch_UpdatedBy",
                table: "RII_BUDGET_PLAN_FISH_BATCH",
                newName: "IX_RII_BUDGET_PLAN_FISH_BATCH_UpdatedBy");

            migrationBuilder.RenameIndex(
                name: "IX_RII_BUDGET_PlanFishBatch_SourceFishBatchId",
                table: "RII_BUDGET_PLAN_FISH_BATCH",
                newName: "IX_RII_BUDGET_PLAN_FISH_BATCH_SourceFishBatchId");

            migrationBuilder.RenameIndex(
                name: "IX_RII_BUDGET_PlanFishBatch_FishStockId",
                table: "RII_BUDGET_PLAN_FISH_BATCH",
                newName: "IX_RII_BUDGET_PLAN_FISH_BATCH_FishStockId");

            migrationBuilder.RenameIndex(
                name: "IX_RII_BUDGET_PlanFishBatch_DeletedBy",
                table: "RII_BUDGET_PLAN_FISH_BATCH",
                newName: "IX_RII_BUDGET_PLAN_FISH_BATCH_DeletedBy");

            migrationBuilder.RenameIndex(
                name: "IX_RII_BUDGET_PlanFishBatch_CreatedBy",
                table: "RII_BUDGET_PLAN_FISH_BATCH",
                newName: "IX_RII_BUDGET_PLAN_FISH_BATCH_CreatedBy");

            migrationBuilder.RenameIndex(
                name: "IX_RII_BUDGET_PlanFishBatch_BudgetPlanProjectId",
                table: "RII_BUDGET_PLAN_FISH_BATCH",
                newName: "IX_RII_BUDGET_PLAN_FISH_BATCH_BudgetPlanProjectId");

            migrationBuilder.RenameIndex(
                name: "IX_RII_BUDGET_PlanFishBatch_BatchCode_Active",
                table: "RII_BUDGET_PLAN_FISH_BATCH",
                newName: "IX_RII_BUDGET_PLAN_FISH_BATCH_BatchCode_Active");

            migrationBuilder.RenameIndex(
                name: "IX_RII_BUDGET_PlanFeedingLine_UpdatedBy",
                table: "RII_BUDGET_PLAN_FEEDING_LINE",
                newName: "IX_RII_BUDGET_PLAN_FEEDING_LINE_UpdatedBy");

            migrationBuilder.RenameIndex(
                name: "IX_RII_BUDGET_PlanFeedingLine_FeedStockId",
                table: "RII_BUDGET_PLAN_FEEDING_LINE",
                newName: "IX_RII_BUDGET_PLAN_FEEDING_LINE_FeedStockId");

            migrationBuilder.RenameIndex(
                name: "IX_RII_BUDGET_PlanFeedingLine_DeletedBy",
                table: "RII_BUDGET_PLAN_FEEDING_LINE",
                newName: "IX_RII_BUDGET_PLAN_FEEDING_LINE_DeletedBy");

            migrationBuilder.RenameIndex(
                name: "IX_RII_BUDGET_PlanFeedingLine_CreatedBy",
                table: "RII_BUDGET_PLAN_FEEDING_LINE",
                newName: "IX_RII_BUDGET_PLAN_FEEDING_LINE_CreatedBy");

            migrationBuilder.RenameIndex(
                name: "IX_RII_BUDGET_PlanFeedingLine_BudgetPlanMonthlyProjectionId",
                table: "RII_BUDGET_PLAN_FEEDING_LINE",
                newName: "IX_RII_BUDGET_PLAN_FEEDING_LINE_BudgetPlanMonthlyProjectionId");

            migrationBuilder.RenameIndex(
                name: "IX_RII_BUDGET_PlanFeedingLine_BudgetPlanId",
                table: "RII_BUDGET_PLAN_FEEDING_LINE",
                newName: "IX_RII_BUDGET_PLAN_FEEDING_LINE_BudgetPlanId");

            migrationBuilder.RenameIndex(
                name: "IX_RII_BUDGET_PlanFeedingLine_BudgetPlanFishBatchId",
                table: "RII_BUDGET_PLAN_FEEDING_LINE",
                newName: "IX_RII_BUDGET_PLAN_FEEDING_LINE_BudgetPlanFishBatchId");

            migrationBuilder.RenameIndex(
                name: "UX_RII_BUDGET_PlanExchangeRate_PeriodCurrency",
                table: "RII_BUDGET_PLAN_EXCHANGE_RATE",
                newName: "UX_RII_BUDGET_PLAN_EXCHANGE_RATE_PeriodCurrency");

            migrationBuilder.RenameIndex(
                name: "IX_RII_BUDGET_PlanExchangeRate_UpdatedBy",
                table: "RII_BUDGET_PLAN_EXCHANGE_RATE",
                newName: "IX_RII_BUDGET_PLAN_EXCHANGE_RATE_UpdatedBy");

            migrationBuilder.RenameIndex(
                name: "IX_RII_BUDGET_PlanExchangeRate_DeletedBy",
                table: "RII_BUDGET_PLAN_EXCHANGE_RATE",
                newName: "IX_RII_BUDGET_PLAN_EXCHANGE_RATE_DeletedBy");

            migrationBuilder.RenameIndex(
                name: "IX_RII_BUDGET_PlanExchangeRate_CreatedBy",
                table: "RII_BUDGET_PLAN_EXCHANGE_RATE",
                newName: "IX_RII_BUDGET_PLAN_EXCHANGE_RATE_CreatedBy");

            migrationBuilder.RenameIndex(
                name: "IX_RII_BUDGET_MortalityRateDefinition_UpdatedBy",
                table: "RII_BUDGET_MORTALITY_RATE_DEFINITION",
                newName: "IX_RII_BUDGET_MORTALITY_RATE_DEFINITION_UpdatedBy");

            migrationBuilder.RenameIndex(
                name: "IX_RII_BUDGET_MortalityRateDefinition_Key_Active",
                table: "RII_BUDGET_MORTALITY_RATE_DEFINITION",
                newName: "IX_RII_BUDGET_MORTALITY_RATE_DEFINITION_Key_Active");

            migrationBuilder.RenameIndex(
                name: "IX_RII_BUDGET_MortalityRateDefinition_DeletedBy",
                table: "RII_BUDGET_MORTALITY_RATE_DEFINITION",
                newName: "IX_RII_BUDGET_MORTALITY_RATE_DEFINITION_DeletedBy");

            migrationBuilder.RenameIndex(
                name: "IX_RII_BUDGET_MortalityRateDefinition_CreatedBy",
                table: "RII_BUDGET_MORTALITY_RATE_DEFINITION",
                newName: "IX_RII_BUDGET_MORTALITY_RATE_DEFINITION_CreatedBy");

            migrationBuilder.RenameIndex(
                name: "IX_RII_BUDGET_MortalityRateDefinition_CalibrationDefinitionId",
                table: "RII_BUDGET_MORTALITY_RATE_DEFINITION",
                newName: "IX_RII_BUDGET_MORTALITY_RATE_DEFINITION_CalibrationDefinitionId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RII_BUDGET_PLAN",
                table: "RII_BUDGET_PLAN",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RII_BUDGET_PLAN_SALES_LINE",
                table: "RII_BUDGET_PLAN_SALES_LINE",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RII_BUDGET_PLAN_PROJECT",
                table: "RII_BUDGET_PLAN_PROJECT",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RII_BUDGET_PLAN_MORTALITY_LINE",
                table: "RII_BUDGET_PLAN_MORTALITY_LINE",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RII_BUDGET_PLAN_MONTHLY_PROJECTION",
                table: "RII_BUDGET_PLAN_MONTHLY_PROJECTION",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RII_BUDGET_PLAN_FISH_BATCH_ADJUSTMENT",
                table: "RII_BUDGET_PLAN_FISH_BATCH_ADJUSTMENT",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RII_BUDGET_PLAN_FISH_BATCH",
                table: "RII_BUDGET_PLAN_FISH_BATCH",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RII_BUDGET_PLAN_FEEDING_LINE",
                table: "RII_BUDGET_PLAN_FEEDING_LINE",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RII_BUDGET_PLAN_EXCHANGE_RATE",
                table: "RII_BUDGET_PLAN_EXCHANGE_RATE",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RII_BUDGET_MORTALITY_RATE_DEFINITION",
                table: "RII_BUDGET_MORTALITY_RATE_DEFINITION",
                column: "Id");

            migrationBuilder.AddCheckConstraint(
                name: "CK_RII_BUDGET_PLAN_SALES_LINE_Month",
                table: "RII_BUDGET_PLAN_SALES_LINE",
                sql: "[Month] BETWEEN 1 AND 12");

            migrationBuilder.AddCheckConstraint(
                name: "CK_RII_BUDGET_PLAN_SALES_LINE_NonNegative",
                table: "RII_BUDGET_PLAN_SALES_LINE",
                sql: "[SalesKg] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_RII_BUDGET_PLAN_PROJECT_SourceType",
                table: "RII_BUDGET_PLAN_PROJECT",
                sql: "[SourceType] IN (0,1)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_RII_BUDGET_PLAN_MORTALITY_LINE_Month",
                table: "RII_BUDGET_PLAN_MORTALITY_LINE",
                sql: "[Month] BETWEEN 1 AND 12");

            migrationBuilder.AddCheckConstraint(
                name: "CK_RII_BUDGET_PLAN_MORTALITY_LINE_NonNegative",
                table: "RII_BUDGET_PLAN_MORTALITY_LINE",
                sql: "[MortalityRatePercent] >= 0 AND [MortalityCount] >= 0 AND [MortalityKg] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_RII_BUDGET_PLAN_MONTHLY_PROJECTION_Month",
                table: "RII_BUDGET_PLAN_MONTHLY_PROJECTION",
                sql: "[Month] BETWEEN 1 AND 12");

            migrationBuilder.AddCheckConstraint(
                name: "CK_RII_BUDGET_PLAN_MONTHLY_PROJECTION_NonNegative",
                table: "RII_BUDGET_PLAN_MONTHLY_PROJECTION",
                sql: "[OpeningLiveCount] >= 0 AND [ClosingLiveCount] >= 0 AND [OpeningBiomassKg] >= 0 AND [ClosingBiomassKg] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_RII_BUDGET_PLAN_FISH_BATCH_ADJUSTMENT_Positive",
                table: "RII_BUDGET_PLAN_FISH_BATCH_ADJUSTMENT",
                sql: "[LiveCount] > 0 AND [AverageGram] >= 0 AND [BiomassKg] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_RII_BUDGET_PLAN_FISH_BATCH_ADJUSTMENT_Type",
                table: "RII_BUDGET_PLAN_FISH_BATCH_ADJUSTMENT",
                sql: "[AdjustmentType] IN (0,1,2,3)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_RII_BUDGET_PLAN_FISH_BATCH_GrowthStartMonth",
                table: "RII_BUDGET_PLAN_FISH_BATCH",
                sql: "[GrowthStartMonth] BETWEEN 1 AND 12");

            migrationBuilder.AddCheckConstraint(
                name: "CK_RII_BUDGET_PLAN_FISH_BATCH_NonNegative",
                table: "RII_BUDGET_PLAN_FISH_BATCH",
                sql: "[InitialLiveCount] >= 0 AND [InitialAverageGram] >= 0 AND [InitialBiomassKg] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_RII_BUDGET_PLAN_FISH_BATCH_SourceType",
                table: "RII_BUDGET_PLAN_FISH_BATCH",
                sql: "[SourceType] IN (0,1)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_RII_BUDGET_PLAN_FEEDING_LINE_Month",
                table: "RII_BUDGET_PLAN_FEEDING_LINE",
                sql: "[Month] BETWEEN 1 AND 12");

            migrationBuilder.AddCheckConstraint(
                name: "CK_RII_BUDGET_PLAN_FEEDING_LINE_NonNegative",
                table: "RII_BUDGET_PLAN_FEEDING_LINE",
                sql: "[FeedAmountRate] >= 0 AND [FeedKg] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_RII_BUDGET_PLAN_EXCHANGE_RATE_Month",
                table: "RII_BUDGET_PLAN_EXCHANGE_RATE",
                sql: "[Month] BETWEEN 1 AND 12");

            migrationBuilder.AddCheckConstraint(
                name: "CK_RII_BUDGET_PLAN_EXCHANGE_RATE_Rate",
                table: "RII_BUDGET_PLAN_EXCHANGE_RATE",
                sql: "[ExchangeRate] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_RII_BUDGET_MORTALITY_RATE_DEFINITION_Rate",
                table: "RII_BUDGET_MORTALITY_RATE_DEFINITION",
                sql: "[MortalityRatePercent] >= 0 AND [MortalityRatePercent] <= 100");

            migrationBuilder.AddForeignKey(
                name: "FK_RII_BUDGET_MORTALITY_RATE_DEFINITION_RII_BUDGET_CALIBRATION_DEFINITION_CalibrationDefinitionId",
                table: "RII_BUDGET_MORTALITY_RATE_DEFINITION",
                column: "CalibrationDefinitionId",
                principalTable: "RII_BUDGET_CALIBRATION_DEFINITION",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RII_BUDGET_MORTALITY_RATE_DEFINITION_RII_STOCK_FishStockId",
                table: "RII_BUDGET_MORTALITY_RATE_DEFINITION",
                column: "FishStockId",
                principalTable: "RII_STOCK",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RII_BUDGET_MORTALITY_RATE_DEFINITION_RII_USERS_CreatedBy",
                table: "RII_BUDGET_MORTALITY_RATE_DEFINITION",
                column: "CreatedBy",
                principalTable: "RII_USERS",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RII_BUDGET_MORTALITY_RATE_DEFINITION_RII_USERS_DeletedBy",
                table: "RII_BUDGET_MORTALITY_RATE_DEFINITION",
                column: "DeletedBy",
                principalTable: "RII_USERS",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RII_BUDGET_MORTALITY_RATE_DEFINITION_RII_USERS_UpdatedBy",
                table: "RII_BUDGET_MORTALITY_RATE_DEFINITION",
                column: "UpdatedBy",
                principalTable: "RII_USERS",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RII_BUDGET_PLAN_RII_USERS_CreatedBy",
                table: "RII_BUDGET_PLAN",
                column: "CreatedBy",
                principalTable: "RII_USERS",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RII_BUDGET_PLAN_RII_USERS_DeletedBy",
                table: "RII_BUDGET_PLAN",
                column: "DeletedBy",
                principalTable: "RII_USERS",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RII_BUDGET_PLAN_RII_USERS_UpdatedBy",
                table: "RII_BUDGET_PLAN",
                column: "UpdatedBy",
                principalTable: "RII_USERS",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RII_BUDGET_PLAN_EXCHANGE_RATE_RII_BUDGET_PLAN_BudgetPlanId",
                table: "RII_BUDGET_PLAN_EXCHANGE_RATE",
                column: "BudgetPlanId",
                principalTable: "RII_BUDGET_PLAN",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RII_BUDGET_PLAN_EXCHANGE_RATE_RII_USERS_CreatedBy",
                table: "RII_BUDGET_PLAN_EXCHANGE_RATE",
                column: "CreatedBy",
                principalTable: "RII_USERS",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RII_BUDGET_PLAN_EXCHANGE_RATE_RII_USERS_DeletedBy",
                table: "RII_BUDGET_PLAN_EXCHANGE_RATE",
                column: "DeletedBy",
                principalTable: "RII_USERS",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RII_BUDGET_PLAN_EXCHANGE_RATE_RII_USERS_UpdatedBy",
                table: "RII_BUDGET_PLAN_EXCHANGE_RATE",
                column: "UpdatedBy",
                principalTable: "RII_USERS",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RII_BUDGET_PLAN_FEEDING_LINE_RII_BUDGET_PLAN_BudgetPlanId",
                table: "RII_BUDGET_PLAN_FEEDING_LINE",
                column: "BudgetPlanId",
                principalTable: "RII_BUDGET_PLAN",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RII_BUDGET_PLAN_FEEDING_LINE_RII_BUDGET_PLAN_FISH_BATCH_BudgetPlanFishBatchId",
                table: "RII_BUDGET_PLAN_FEEDING_LINE",
                column: "BudgetPlanFishBatchId",
                principalTable: "RII_BUDGET_PLAN_FISH_BATCH",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RII_BUDGET_PLAN_FEEDING_LINE_RII_BUDGET_PLAN_MONTHLY_PROJECTION_BudgetPlanMonthlyProjectionId",
                table: "RII_BUDGET_PLAN_FEEDING_LINE",
                column: "BudgetPlanMonthlyProjectionId",
                principalTable: "RII_BUDGET_PLAN_MONTHLY_PROJECTION",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RII_BUDGET_PLAN_FEEDING_LINE_RII_STOCK_FeedStockId",
                table: "RII_BUDGET_PLAN_FEEDING_LINE",
                column: "FeedStockId",
                principalTable: "RII_STOCK",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RII_BUDGET_PLAN_FEEDING_LINE_RII_USERS_CreatedBy",
                table: "RII_BUDGET_PLAN_FEEDING_LINE",
                column: "CreatedBy",
                principalTable: "RII_USERS",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RII_BUDGET_PLAN_FEEDING_LINE_RII_USERS_DeletedBy",
                table: "RII_BUDGET_PLAN_FEEDING_LINE",
                column: "DeletedBy",
                principalTable: "RII_USERS",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RII_BUDGET_PLAN_FEEDING_LINE_RII_USERS_UpdatedBy",
                table: "RII_BUDGET_PLAN_FEEDING_LINE",
                column: "UpdatedBy",
                principalTable: "RII_USERS",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RII_BUDGET_PLAN_FISH_BATCH_RII_BUDGET_PLAN_BudgetPlanId",
                table: "RII_BUDGET_PLAN_FISH_BATCH",
                column: "BudgetPlanId",
                principalTable: "RII_BUDGET_PLAN",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RII_BUDGET_PLAN_FISH_BATCH_RII_BUDGET_PLAN_PROJECT_BudgetPlanProjectId",
                table: "RII_BUDGET_PLAN_FISH_BATCH",
                column: "BudgetPlanProjectId",
                principalTable: "RII_BUDGET_PLAN_PROJECT",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RII_BUDGET_PLAN_FISH_BATCH_RII_FishBatch_SourceFishBatchId",
                table: "RII_BUDGET_PLAN_FISH_BATCH",
                column: "SourceFishBatchId",
                principalTable: "RII_FishBatch",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RII_BUDGET_PLAN_FISH_BATCH_RII_STOCK_FishStockId",
                table: "RII_BUDGET_PLAN_FISH_BATCH",
                column: "FishStockId",
                principalTable: "RII_STOCK",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RII_BUDGET_PLAN_FISH_BATCH_RII_USERS_CreatedBy",
                table: "RII_BUDGET_PLAN_FISH_BATCH",
                column: "CreatedBy",
                principalTable: "RII_USERS",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RII_BUDGET_PLAN_FISH_BATCH_RII_USERS_DeletedBy",
                table: "RII_BUDGET_PLAN_FISH_BATCH",
                column: "DeletedBy",
                principalTable: "RII_USERS",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RII_BUDGET_PLAN_FISH_BATCH_RII_USERS_UpdatedBy",
                table: "RII_BUDGET_PLAN_FISH_BATCH",
                column: "UpdatedBy",
                principalTable: "RII_USERS",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RII_BUDGET_PLAN_FISH_BATCH_ADJUSTMENT_RII_BUDGET_PLAN_BudgetPlanId",
                table: "RII_BUDGET_PLAN_FISH_BATCH_ADJUSTMENT",
                column: "BudgetPlanId",
                principalTable: "RII_BUDGET_PLAN",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RII_BUDGET_PLAN_FISH_BATCH_ADJUSTMENT_RII_BUDGET_PLAN_FISH_BATCH_BudgetPlanFishBatchId",
                table: "RII_BUDGET_PLAN_FISH_BATCH_ADJUSTMENT",
                column: "BudgetPlanFishBatchId",
                principalTable: "RII_BUDGET_PLAN_FISH_BATCH",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RII_BUDGET_PLAN_FISH_BATCH_ADJUSTMENT_RII_USERS_CreatedBy",
                table: "RII_BUDGET_PLAN_FISH_BATCH_ADJUSTMENT",
                column: "CreatedBy",
                principalTable: "RII_USERS",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RII_BUDGET_PLAN_FISH_BATCH_ADJUSTMENT_RII_USERS_DeletedBy",
                table: "RII_BUDGET_PLAN_FISH_BATCH_ADJUSTMENT",
                column: "DeletedBy",
                principalTable: "RII_USERS",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RII_BUDGET_PLAN_FISH_BATCH_ADJUSTMENT_RII_USERS_UpdatedBy",
                table: "RII_BUDGET_PLAN_FISH_BATCH_ADJUSTMENT",
                column: "UpdatedBy",
                principalTable: "RII_USERS",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RII_BUDGET_PLAN_MONTHLY_PROJECTION_RII_BUDGET_CALIBRATION_DEFINITION_CalibrationDefinitionId",
                table: "RII_BUDGET_PLAN_MONTHLY_PROJECTION",
                column: "CalibrationDefinitionId",
                principalTable: "RII_BUDGET_CALIBRATION_DEFINITION",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RII_BUDGET_PLAN_MONTHLY_PROJECTION_RII_BUDGET_PLAN_BudgetPlanId",
                table: "RII_BUDGET_PLAN_MONTHLY_PROJECTION",
                column: "BudgetPlanId",
                principalTable: "RII_BUDGET_PLAN",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RII_BUDGET_PLAN_MONTHLY_PROJECTION_RII_BUDGET_PLAN_FISH_BATCH_BudgetPlanFishBatchId",
                table: "RII_BUDGET_PLAN_MONTHLY_PROJECTION",
                column: "BudgetPlanFishBatchId",
                principalTable: "RII_BUDGET_PLAN_FISH_BATCH",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RII_BUDGET_PLAN_MONTHLY_PROJECTION_RII_BUDGET_WATER_TEMPERATURE_WaterTemperatureId",
                table: "RII_BUDGET_PLAN_MONTHLY_PROJECTION",
                column: "WaterTemperatureId",
                principalTable: "RII_BUDGET_WATER_TEMPERATURE",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RII_BUDGET_PLAN_MONTHLY_PROJECTION_RII_USERS_CreatedBy",
                table: "RII_BUDGET_PLAN_MONTHLY_PROJECTION",
                column: "CreatedBy",
                principalTable: "RII_USERS",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RII_BUDGET_PLAN_MONTHLY_PROJECTION_RII_USERS_DeletedBy",
                table: "RII_BUDGET_PLAN_MONTHLY_PROJECTION",
                column: "DeletedBy",
                principalTable: "RII_USERS",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RII_BUDGET_PLAN_MONTHLY_PROJECTION_RII_USERS_UpdatedBy",
                table: "RII_BUDGET_PLAN_MONTHLY_PROJECTION",
                column: "UpdatedBy",
                principalTable: "RII_USERS",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RII_BUDGET_PLAN_MORTALITY_LINE_RII_BUDGET_PLAN_BudgetPlanId",
                table: "RII_BUDGET_PLAN_MORTALITY_LINE",
                column: "BudgetPlanId",
                principalTable: "RII_BUDGET_PLAN",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RII_BUDGET_PLAN_MORTALITY_LINE_RII_BUDGET_PLAN_FISH_BATCH_BudgetPlanFishBatchId",
                table: "RII_BUDGET_PLAN_MORTALITY_LINE",
                column: "BudgetPlanFishBatchId",
                principalTable: "RII_BUDGET_PLAN_FISH_BATCH",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RII_BUDGET_PLAN_MORTALITY_LINE_RII_BUDGET_PLAN_MONTHLY_PROJECTION_BudgetPlanMonthlyProjectionId",
                table: "RII_BUDGET_PLAN_MORTALITY_LINE",
                column: "BudgetPlanMonthlyProjectionId",
                principalTable: "RII_BUDGET_PLAN_MONTHLY_PROJECTION",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RII_BUDGET_PLAN_MORTALITY_LINE_RII_USERS_CreatedBy",
                table: "RII_BUDGET_PLAN_MORTALITY_LINE",
                column: "CreatedBy",
                principalTable: "RII_USERS",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RII_BUDGET_PLAN_MORTALITY_LINE_RII_USERS_DeletedBy",
                table: "RII_BUDGET_PLAN_MORTALITY_LINE",
                column: "DeletedBy",
                principalTable: "RII_USERS",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RII_BUDGET_PLAN_MORTALITY_LINE_RII_USERS_UpdatedBy",
                table: "RII_BUDGET_PLAN_MORTALITY_LINE",
                column: "UpdatedBy",
                principalTable: "RII_USERS",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RII_BUDGET_PLAN_PROJECT_RII_BUDGET_PLAN_BudgetPlanId",
                table: "RII_BUDGET_PLAN_PROJECT",
                column: "BudgetPlanId",
                principalTable: "RII_BUDGET_PLAN",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RII_BUDGET_PLAN_PROJECT_RII_Project_SourceProjectId",
                table: "RII_BUDGET_PLAN_PROJECT",
                column: "SourceProjectId",
                principalTable: "RII_Project",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RII_BUDGET_PLAN_PROJECT_RII_USERS_CreatedBy",
                table: "RII_BUDGET_PLAN_PROJECT",
                column: "CreatedBy",
                principalTable: "RII_USERS",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RII_BUDGET_PLAN_PROJECT_RII_USERS_DeletedBy",
                table: "RII_BUDGET_PLAN_PROJECT",
                column: "DeletedBy",
                principalTable: "RII_USERS",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RII_BUDGET_PLAN_PROJECT_RII_USERS_UpdatedBy",
                table: "RII_BUDGET_PLAN_PROJECT",
                column: "UpdatedBy",
                principalTable: "RII_USERS",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RII_BUDGET_PLAN_SALES_LINE_RII_BUDGET_PLAN_BudgetPlanId",
                table: "RII_BUDGET_PLAN_SALES_LINE",
                column: "BudgetPlanId",
                principalTable: "RII_BUDGET_PLAN",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RII_BUDGET_PLAN_SALES_LINE_RII_BUDGET_PLAN_FISH_BATCH_BudgetPlanFishBatchId",
                table: "RII_BUDGET_PLAN_SALES_LINE",
                column: "BudgetPlanFishBatchId",
                principalTable: "RII_BUDGET_PLAN_FISH_BATCH",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RII_BUDGET_PLAN_SALES_LINE_RII_USERS_CreatedBy",
                table: "RII_BUDGET_PLAN_SALES_LINE",
                column: "CreatedBy",
                principalTable: "RII_USERS",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RII_BUDGET_PLAN_SALES_LINE_RII_USERS_DeletedBy",
                table: "RII_BUDGET_PLAN_SALES_LINE",
                column: "DeletedBy",
                principalTable: "RII_USERS",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RII_BUDGET_PLAN_SALES_LINE_RII_USERS_UpdatedBy",
                table: "RII_BUDGET_PLAN_SALES_LINE",
                column: "UpdatedBy",
                principalTable: "RII_USERS",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RII_BUDGET_MORTALITY_RATE_DEFINITION_RII_BUDGET_CALIBRATION_DEFINITION_CalibrationDefinitionId",
                table: "RII_BUDGET_MORTALITY_RATE_DEFINITION");

            migrationBuilder.DropForeignKey(
                name: "FK_RII_BUDGET_MORTALITY_RATE_DEFINITION_RII_STOCK_FishStockId",
                table: "RII_BUDGET_MORTALITY_RATE_DEFINITION");

            migrationBuilder.DropForeignKey(
                name: "FK_RII_BUDGET_MORTALITY_RATE_DEFINITION_RII_USERS_CreatedBy",
                table: "RII_BUDGET_MORTALITY_RATE_DEFINITION");

            migrationBuilder.DropForeignKey(
                name: "FK_RII_BUDGET_MORTALITY_RATE_DEFINITION_RII_USERS_DeletedBy",
                table: "RII_BUDGET_MORTALITY_RATE_DEFINITION");

            migrationBuilder.DropForeignKey(
                name: "FK_RII_BUDGET_MORTALITY_RATE_DEFINITION_RII_USERS_UpdatedBy",
                table: "RII_BUDGET_MORTALITY_RATE_DEFINITION");

            migrationBuilder.DropForeignKey(
                name: "FK_RII_BUDGET_PLAN_RII_USERS_CreatedBy",
                table: "RII_BUDGET_PLAN");

            migrationBuilder.DropForeignKey(
                name: "FK_RII_BUDGET_PLAN_RII_USERS_DeletedBy",
                table: "RII_BUDGET_PLAN");

            migrationBuilder.DropForeignKey(
                name: "FK_RII_BUDGET_PLAN_RII_USERS_UpdatedBy",
                table: "RII_BUDGET_PLAN");

            migrationBuilder.DropForeignKey(
                name: "FK_RII_BUDGET_PLAN_EXCHANGE_RATE_RII_BUDGET_PLAN_BudgetPlanId",
                table: "RII_BUDGET_PLAN_EXCHANGE_RATE");

            migrationBuilder.DropForeignKey(
                name: "FK_RII_BUDGET_PLAN_EXCHANGE_RATE_RII_USERS_CreatedBy",
                table: "RII_BUDGET_PLAN_EXCHANGE_RATE");

            migrationBuilder.DropForeignKey(
                name: "FK_RII_BUDGET_PLAN_EXCHANGE_RATE_RII_USERS_DeletedBy",
                table: "RII_BUDGET_PLAN_EXCHANGE_RATE");

            migrationBuilder.DropForeignKey(
                name: "FK_RII_BUDGET_PLAN_EXCHANGE_RATE_RII_USERS_UpdatedBy",
                table: "RII_BUDGET_PLAN_EXCHANGE_RATE");

            migrationBuilder.DropForeignKey(
                name: "FK_RII_BUDGET_PLAN_FEEDING_LINE_RII_BUDGET_PLAN_BudgetPlanId",
                table: "RII_BUDGET_PLAN_FEEDING_LINE");

            migrationBuilder.DropForeignKey(
                name: "FK_RII_BUDGET_PLAN_FEEDING_LINE_RII_BUDGET_PLAN_FISH_BATCH_BudgetPlanFishBatchId",
                table: "RII_BUDGET_PLAN_FEEDING_LINE");

            migrationBuilder.DropForeignKey(
                name: "FK_RII_BUDGET_PLAN_FEEDING_LINE_RII_BUDGET_PLAN_MONTHLY_PROJECTION_BudgetPlanMonthlyProjectionId",
                table: "RII_BUDGET_PLAN_FEEDING_LINE");

            migrationBuilder.DropForeignKey(
                name: "FK_RII_BUDGET_PLAN_FEEDING_LINE_RII_STOCK_FeedStockId",
                table: "RII_BUDGET_PLAN_FEEDING_LINE");

            migrationBuilder.DropForeignKey(
                name: "FK_RII_BUDGET_PLAN_FEEDING_LINE_RII_USERS_CreatedBy",
                table: "RII_BUDGET_PLAN_FEEDING_LINE");

            migrationBuilder.DropForeignKey(
                name: "FK_RII_BUDGET_PLAN_FEEDING_LINE_RII_USERS_DeletedBy",
                table: "RII_BUDGET_PLAN_FEEDING_LINE");

            migrationBuilder.DropForeignKey(
                name: "FK_RII_BUDGET_PLAN_FEEDING_LINE_RII_USERS_UpdatedBy",
                table: "RII_BUDGET_PLAN_FEEDING_LINE");

            migrationBuilder.DropForeignKey(
                name: "FK_RII_BUDGET_PLAN_FISH_BATCH_RII_BUDGET_PLAN_BudgetPlanId",
                table: "RII_BUDGET_PLAN_FISH_BATCH");

            migrationBuilder.DropForeignKey(
                name: "FK_RII_BUDGET_PLAN_FISH_BATCH_RII_BUDGET_PLAN_PROJECT_BudgetPlanProjectId",
                table: "RII_BUDGET_PLAN_FISH_BATCH");

            migrationBuilder.DropForeignKey(
                name: "FK_RII_BUDGET_PLAN_FISH_BATCH_RII_FishBatch_SourceFishBatchId",
                table: "RII_BUDGET_PLAN_FISH_BATCH");

            migrationBuilder.DropForeignKey(
                name: "FK_RII_BUDGET_PLAN_FISH_BATCH_RII_STOCK_FishStockId",
                table: "RII_BUDGET_PLAN_FISH_BATCH");

            migrationBuilder.DropForeignKey(
                name: "FK_RII_BUDGET_PLAN_FISH_BATCH_RII_USERS_CreatedBy",
                table: "RII_BUDGET_PLAN_FISH_BATCH");

            migrationBuilder.DropForeignKey(
                name: "FK_RII_BUDGET_PLAN_FISH_BATCH_RII_USERS_DeletedBy",
                table: "RII_BUDGET_PLAN_FISH_BATCH");

            migrationBuilder.DropForeignKey(
                name: "FK_RII_BUDGET_PLAN_FISH_BATCH_RII_USERS_UpdatedBy",
                table: "RII_BUDGET_PLAN_FISH_BATCH");

            migrationBuilder.DropForeignKey(
                name: "FK_RII_BUDGET_PLAN_FISH_BATCH_ADJUSTMENT_RII_BUDGET_PLAN_BudgetPlanId",
                table: "RII_BUDGET_PLAN_FISH_BATCH_ADJUSTMENT");

            migrationBuilder.DropForeignKey(
                name: "FK_RII_BUDGET_PLAN_FISH_BATCH_ADJUSTMENT_RII_BUDGET_PLAN_FISH_BATCH_BudgetPlanFishBatchId",
                table: "RII_BUDGET_PLAN_FISH_BATCH_ADJUSTMENT");

            migrationBuilder.DropForeignKey(
                name: "FK_RII_BUDGET_PLAN_FISH_BATCH_ADJUSTMENT_RII_USERS_CreatedBy",
                table: "RII_BUDGET_PLAN_FISH_BATCH_ADJUSTMENT");

            migrationBuilder.DropForeignKey(
                name: "FK_RII_BUDGET_PLAN_FISH_BATCH_ADJUSTMENT_RII_USERS_DeletedBy",
                table: "RII_BUDGET_PLAN_FISH_BATCH_ADJUSTMENT");

            migrationBuilder.DropForeignKey(
                name: "FK_RII_BUDGET_PLAN_FISH_BATCH_ADJUSTMENT_RII_USERS_UpdatedBy",
                table: "RII_BUDGET_PLAN_FISH_BATCH_ADJUSTMENT");

            migrationBuilder.DropForeignKey(
                name: "FK_RII_BUDGET_PLAN_MONTHLY_PROJECTION_RII_BUDGET_CALIBRATION_DEFINITION_CalibrationDefinitionId",
                table: "RII_BUDGET_PLAN_MONTHLY_PROJECTION");

            migrationBuilder.DropForeignKey(
                name: "FK_RII_BUDGET_PLAN_MONTHLY_PROJECTION_RII_BUDGET_PLAN_BudgetPlanId",
                table: "RII_BUDGET_PLAN_MONTHLY_PROJECTION");

            migrationBuilder.DropForeignKey(
                name: "FK_RII_BUDGET_PLAN_MONTHLY_PROJECTION_RII_BUDGET_PLAN_FISH_BATCH_BudgetPlanFishBatchId",
                table: "RII_BUDGET_PLAN_MONTHLY_PROJECTION");

            migrationBuilder.DropForeignKey(
                name: "FK_RII_BUDGET_PLAN_MONTHLY_PROJECTION_RII_BUDGET_WATER_TEMPERATURE_WaterTemperatureId",
                table: "RII_BUDGET_PLAN_MONTHLY_PROJECTION");

            migrationBuilder.DropForeignKey(
                name: "FK_RII_BUDGET_PLAN_MONTHLY_PROJECTION_RII_USERS_CreatedBy",
                table: "RII_BUDGET_PLAN_MONTHLY_PROJECTION");

            migrationBuilder.DropForeignKey(
                name: "FK_RII_BUDGET_PLAN_MONTHLY_PROJECTION_RII_USERS_DeletedBy",
                table: "RII_BUDGET_PLAN_MONTHLY_PROJECTION");

            migrationBuilder.DropForeignKey(
                name: "FK_RII_BUDGET_PLAN_MONTHLY_PROJECTION_RII_USERS_UpdatedBy",
                table: "RII_BUDGET_PLAN_MONTHLY_PROJECTION");

            migrationBuilder.DropForeignKey(
                name: "FK_RII_BUDGET_PLAN_MORTALITY_LINE_RII_BUDGET_PLAN_BudgetPlanId",
                table: "RII_BUDGET_PLAN_MORTALITY_LINE");

            migrationBuilder.DropForeignKey(
                name: "FK_RII_BUDGET_PLAN_MORTALITY_LINE_RII_BUDGET_PLAN_FISH_BATCH_BudgetPlanFishBatchId",
                table: "RII_BUDGET_PLAN_MORTALITY_LINE");

            migrationBuilder.DropForeignKey(
                name: "FK_RII_BUDGET_PLAN_MORTALITY_LINE_RII_BUDGET_PLAN_MONTHLY_PROJECTION_BudgetPlanMonthlyProjectionId",
                table: "RII_BUDGET_PLAN_MORTALITY_LINE");

            migrationBuilder.DropForeignKey(
                name: "FK_RII_BUDGET_PLAN_MORTALITY_LINE_RII_USERS_CreatedBy",
                table: "RII_BUDGET_PLAN_MORTALITY_LINE");

            migrationBuilder.DropForeignKey(
                name: "FK_RII_BUDGET_PLAN_MORTALITY_LINE_RII_USERS_DeletedBy",
                table: "RII_BUDGET_PLAN_MORTALITY_LINE");

            migrationBuilder.DropForeignKey(
                name: "FK_RII_BUDGET_PLAN_MORTALITY_LINE_RII_USERS_UpdatedBy",
                table: "RII_BUDGET_PLAN_MORTALITY_LINE");

            migrationBuilder.DropForeignKey(
                name: "FK_RII_BUDGET_PLAN_PROJECT_RII_BUDGET_PLAN_BudgetPlanId",
                table: "RII_BUDGET_PLAN_PROJECT");

            migrationBuilder.DropForeignKey(
                name: "FK_RII_BUDGET_PLAN_PROJECT_RII_Project_SourceProjectId",
                table: "RII_BUDGET_PLAN_PROJECT");

            migrationBuilder.DropForeignKey(
                name: "FK_RII_BUDGET_PLAN_PROJECT_RII_USERS_CreatedBy",
                table: "RII_BUDGET_PLAN_PROJECT");

            migrationBuilder.DropForeignKey(
                name: "FK_RII_BUDGET_PLAN_PROJECT_RII_USERS_DeletedBy",
                table: "RII_BUDGET_PLAN_PROJECT");

            migrationBuilder.DropForeignKey(
                name: "FK_RII_BUDGET_PLAN_PROJECT_RII_USERS_UpdatedBy",
                table: "RII_BUDGET_PLAN_PROJECT");

            migrationBuilder.DropForeignKey(
                name: "FK_RII_BUDGET_PLAN_SALES_LINE_RII_BUDGET_PLAN_BudgetPlanId",
                table: "RII_BUDGET_PLAN_SALES_LINE");

            migrationBuilder.DropForeignKey(
                name: "FK_RII_BUDGET_PLAN_SALES_LINE_RII_BUDGET_PLAN_FISH_BATCH_BudgetPlanFishBatchId",
                table: "RII_BUDGET_PLAN_SALES_LINE");

            migrationBuilder.DropForeignKey(
                name: "FK_RII_BUDGET_PLAN_SALES_LINE_RII_USERS_CreatedBy",
                table: "RII_BUDGET_PLAN_SALES_LINE");

            migrationBuilder.DropForeignKey(
                name: "FK_RII_BUDGET_PLAN_SALES_LINE_RII_USERS_DeletedBy",
                table: "RII_BUDGET_PLAN_SALES_LINE");

            migrationBuilder.DropForeignKey(
                name: "FK_RII_BUDGET_PLAN_SALES_LINE_RII_USERS_UpdatedBy",
                table: "RII_BUDGET_PLAN_SALES_LINE");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RII_BUDGET_PLAN",
                table: "RII_BUDGET_PLAN");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RII_BUDGET_PLAN_SALES_LINE",
                table: "RII_BUDGET_PLAN_SALES_LINE");

            migrationBuilder.DropCheckConstraint(
                name: "CK_RII_BUDGET_PLAN_SALES_LINE_Month",
                table: "RII_BUDGET_PLAN_SALES_LINE");

            migrationBuilder.DropCheckConstraint(
                name: "CK_RII_BUDGET_PLAN_SALES_LINE_NonNegative",
                table: "RII_BUDGET_PLAN_SALES_LINE");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RII_BUDGET_PLAN_PROJECT",
                table: "RII_BUDGET_PLAN_PROJECT");

            migrationBuilder.DropCheckConstraint(
                name: "CK_RII_BUDGET_PLAN_PROJECT_SourceType",
                table: "RII_BUDGET_PLAN_PROJECT");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RII_BUDGET_PLAN_MORTALITY_LINE",
                table: "RII_BUDGET_PLAN_MORTALITY_LINE");

            migrationBuilder.DropCheckConstraint(
                name: "CK_RII_BUDGET_PLAN_MORTALITY_LINE_Month",
                table: "RII_BUDGET_PLAN_MORTALITY_LINE");

            migrationBuilder.DropCheckConstraint(
                name: "CK_RII_BUDGET_PLAN_MORTALITY_LINE_NonNegative",
                table: "RII_BUDGET_PLAN_MORTALITY_LINE");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RII_BUDGET_PLAN_MONTHLY_PROJECTION",
                table: "RII_BUDGET_PLAN_MONTHLY_PROJECTION");

            migrationBuilder.DropCheckConstraint(
                name: "CK_RII_BUDGET_PLAN_MONTHLY_PROJECTION_Month",
                table: "RII_BUDGET_PLAN_MONTHLY_PROJECTION");

            migrationBuilder.DropCheckConstraint(
                name: "CK_RII_BUDGET_PLAN_MONTHLY_PROJECTION_NonNegative",
                table: "RII_BUDGET_PLAN_MONTHLY_PROJECTION");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RII_BUDGET_PLAN_FISH_BATCH_ADJUSTMENT",
                table: "RII_BUDGET_PLAN_FISH_BATCH_ADJUSTMENT");

            migrationBuilder.DropCheckConstraint(
                name: "CK_RII_BUDGET_PLAN_FISH_BATCH_ADJUSTMENT_Positive",
                table: "RII_BUDGET_PLAN_FISH_BATCH_ADJUSTMENT");

            migrationBuilder.DropCheckConstraint(
                name: "CK_RII_BUDGET_PLAN_FISH_BATCH_ADJUSTMENT_Type",
                table: "RII_BUDGET_PLAN_FISH_BATCH_ADJUSTMENT");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RII_BUDGET_PLAN_FISH_BATCH",
                table: "RII_BUDGET_PLAN_FISH_BATCH");

            migrationBuilder.DropCheckConstraint(
                name: "CK_RII_BUDGET_PLAN_FISH_BATCH_GrowthStartMonth",
                table: "RII_BUDGET_PLAN_FISH_BATCH");

            migrationBuilder.DropCheckConstraint(
                name: "CK_RII_BUDGET_PLAN_FISH_BATCH_NonNegative",
                table: "RII_BUDGET_PLAN_FISH_BATCH");

            migrationBuilder.DropCheckConstraint(
                name: "CK_RII_BUDGET_PLAN_FISH_BATCH_SourceType",
                table: "RII_BUDGET_PLAN_FISH_BATCH");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RII_BUDGET_PLAN_FEEDING_LINE",
                table: "RII_BUDGET_PLAN_FEEDING_LINE");

            migrationBuilder.DropCheckConstraint(
                name: "CK_RII_BUDGET_PLAN_FEEDING_LINE_Month",
                table: "RII_BUDGET_PLAN_FEEDING_LINE");

            migrationBuilder.DropCheckConstraint(
                name: "CK_RII_BUDGET_PLAN_FEEDING_LINE_NonNegative",
                table: "RII_BUDGET_PLAN_FEEDING_LINE");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RII_BUDGET_PLAN_EXCHANGE_RATE",
                table: "RII_BUDGET_PLAN_EXCHANGE_RATE");

            migrationBuilder.DropCheckConstraint(
                name: "CK_RII_BUDGET_PLAN_EXCHANGE_RATE_Month",
                table: "RII_BUDGET_PLAN_EXCHANGE_RATE");

            migrationBuilder.DropCheckConstraint(
                name: "CK_RII_BUDGET_PLAN_EXCHANGE_RATE_Rate",
                table: "RII_BUDGET_PLAN_EXCHANGE_RATE");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RII_BUDGET_MORTALITY_RATE_DEFINITION",
                table: "RII_BUDGET_MORTALITY_RATE_DEFINITION");

            migrationBuilder.DropCheckConstraint(
                name: "CK_RII_BUDGET_MORTALITY_RATE_DEFINITION_Rate",
                table: "RII_BUDGET_MORTALITY_RATE_DEFINITION");

            migrationBuilder.RenameTable(
                name: "RII_BUDGET_PLAN",
                newName: "RII_BUDGET_Plan");

            migrationBuilder.RenameTable(
                name: "RII_BUDGET_PLAN_SALES_LINE",
                newName: "RII_BUDGET_PlanSalesLine");

            migrationBuilder.RenameTable(
                name: "RII_BUDGET_PLAN_PROJECT",
                newName: "RII_BUDGET_PlanProject");

            migrationBuilder.RenameTable(
                name: "RII_BUDGET_PLAN_MORTALITY_LINE",
                newName: "RII_BUDGET_PlanMortalityLine");

            migrationBuilder.RenameTable(
                name: "RII_BUDGET_PLAN_MONTHLY_PROJECTION",
                newName: "RII_BUDGET_PlanMonthlyProjection");

            migrationBuilder.RenameTable(
                name: "RII_BUDGET_PLAN_FISH_BATCH_ADJUSTMENT",
                newName: "RII_BUDGET_PlanFishBatchAdjustment");

            migrationBuilder.RenameTable(
                name: "RII_BUDGET_PLAN_FISH_BATCH",
                newName: "RII_BUDGET_PlanFishBatch");

            migrationBuilder.RenameTable(
                name: "RII_BUDGET_PLAN_FEEDING_LINE",
                newName: "RII_BUDGET_PlanFeedingLine");

            migrationBuilder.RenameTable(
                name: "RII_BUDGET_PLAN_EXCHANGE_RATE",
                newName: "RII_BUDGET_PlanExchangeRate");

            migrationBuilder.RenameTable(
                name: "RII_BUDGET_MORTALITY_RATE_DEFINITION",
                newName: "RII_BUDGET_MortalityRateDefinition");

            migrationBuilder.RenameIndex(
                name: "UX_RII_BUDGET_PLAN_BudgetNo_Active",
                table: "RII_BUDGET_Plan",
                newName: "UX_RII_BUDGET_Plan_BudgetNo_Active");

            migrationBuilder.RenameIndex(
                name: "UX_RII_BUDGET_PLAN_BudgetCode_Active",
                table: "RII_BUDGET_Plan",
                newName: "UX_RII_BUDGET_Plan_BudgetCode_Active");

            migrationBuilder.RenameIndex(
                name: "IX_RII_BUDGET_PLAN_UpdatedBy",
                table: "RII_BUDGET_Plan",
                newName: "IX_RII_BUDGET_Plan_UpdatedBy");

            migrationBuilder.RenameIndex(
                name: "IX_RII_BUDGET_PLAN_DeletedBy",
                table: "RII_BUDGET_Plan",
                newName: "IX_RII_BUDGET_Plan_DeletedBy");

            migrationBuilder.RenameIndex(
                name: "IX_RII_BUDGET_PLAN_CreatedBy",
                table: "RII_BUDGET_Plan",
                newName: "IX_RII_BUDGET_Plan_CreatedBy");

            migrationBuilder.RenameIndex(
                name: "IX_RII_BUDGET_PLAN_SALES_LINE_UpdatedBy",
                table: "RII_BUDGET_PlanSalesLine",
                newName: "IX_RII_BUDGET_PlanSalesLine_UpdatedBy");

            migrationBuilder.RenameIndex(
                name: "IX_RII_BUDGET_PLAN_SALES_LINE_Period_Active",
                table: "RII_BUDGET_PlanSalesLine",
                newName: "IX_RII_BUDGET_PlanSalesLine_Period_Active");

            migrationBuilder.RenameIndex(
                name: "IX_RII_BUDGET_PLAN_SALES_LINE_DeletedBy",
                table: "RII_BUDGET_PlanSalesLine",
                newName: "IX_RII_BUDGET_PlanSalesLine_DeletedBy");

            migrationBuilder.RenameIndex(
                name: "IX_RII_BUDGET_PLAN_SALES_LINE_CreatedBy",
                table: "RII_BUDGET_PlanSalesLine",
                newName: "IX_RII_BUDGET_PlanSalesLine_CreatedBy");

            migrationBuilder.RenameIndex(
                name: "IX_RII_BUDGET_PLAN_SALES_LINE_BudgetPlanFishBatchId",
                table: "RII_BUDGET_PlanSalesLine",
                newName: "IX_RII_BUDGET_PlanSalesLine_BudgetPlanFishBatchId");

            migrationBuilder.RenameIndex(
                name: "IX_RII_BUDGET_PLAN_PROJECT_UpdatedBy",
                table: "RII_BUDGET_PlanProject",
                newName: "IX_RII_BUDGET_PlanProject_UpdatedBy");

            migrationBuilder.RenameIndex(
                name: "IX_RII_BUDGET_PLAN_PROJECT_SourceProjectId",
                table: "RII_BUDGET_PlanProject",
                newName: "IX_RII_BUDGET_PlanProject_SourceProjectId");

            migrationBuilder.RenameIndex(
                name: "IX_RII_BUDGET_PLAN_PROJECT_ProjectCode_Active",
                table: "RII_BUDGET_PlanProject",
                newName: "IX_RII_BUDGET_PlanProject_ProjectCode_Active");

            migrationBuilder.RenameIndex(
                name: "IX_RII_BUDGET_PLAN_PROJECT_DeletedBy",
                table: "RII_BUDGET_PlanProject",
                newName: "IX_RII_BUDGET_PlanProject_DeletedBy");

            migrationBuilder.RenameIndex(
                name: "IX_RII_BUDGET_PLAN_PROJECT_CreatedBy",
                table: "RII_BUDGET_PlanProject",
                newName: "IX_RII_BUDGET_PlanProject_CreatedBy");

            migrationBuilder.RenameIndex(
                name: "IX_RII_BUDGET_PLAN_MORTALITY_LINE_UpdatedBy",
                table: "RII_BUDGET_PlanMortalityLine",
                newName: "IX_RII_BUDGET_PlanMortalityLine_UpdatedBy");

            migrationBuilder.RenameIndex(
                name: "IX_RII_BUDGET_PLAN_MORTALITY_LINE_DeletedBy",
                table: "RII_BUDGET_PlanMortalityLine",
                newName: "IX_RII_BUDGET_PlanMortalityLine_DeletedBy");

            migrationBuilder.RenameIndex(
                name: "IX_RII_BUDGET_PLAN_MORTALITY_LINE_CreatedBy",
                table: "RII_BUDGET_PlanMortalityLine",
                newName: "IX_RII_BUDGET_PlanMortalityLine_CreatedBy");

            migrationBuilder.RenameIndex(
                name: "IX_RII_BUDGET_PLAN_MORTALITY_LINE_BudgetPlanMonthlyProjectionId",
                table: "RII_BUDGET_PlanMortalityLine",
                newName: "IX_RII_BUDGET_PlanMortalityLine_BudgetPlanMonthlyProjectionId");

            migrationBuilder.RenameIndex(
                name: "IX_RII_BUDGET_PLAN_MORTALITY_LINE_BudgetPlanId",
                table: "RII_BUDGET_PlanMortalityLine",
                newName: "IX_RII_BUDGET_PlanMortalityLine_BudgetPlanId");

            migrationBuilder.RenameIndex(
                name: "IX_RII_BUDGET_PLAN_MORTALITY_LINE_BudgetPlanFishBatchId",
                table: "RII_BUDGET_PlanMortalityLine",
                newName: "IX_RII_BUDGET_PlanMortalityLine_BudgetPlanFishBatchId");

            migrationBuilder.RenameIndex(
                name: "UX_RII_BUDGET_PLAN_MONTHLY_PROJECTION_Period_Active",
                table: "RII_BUDGET_PlanMonthlyProjection",
                newName: "UX_RII_BUDGET_PlanMonthlyProjection_Period_Active");

            migrationBuilder.RenameIndex(
                name: "IX_RII_BUDGET_PLAN_MONTHLY_PROJECTION_WaterTemperatureId",
                table: "RII_BUDGET_PlanMonthlyProjection",
                newName: "IX_RII_BUDGET_PlanMonthlyProjection_WaterTemperatureId");

            migrationBuilder.RenameIndex(
                name: "IX_RII_BUDGET_PLAN_MONTHLY_PROJECTION_UpdatedBy",
                table: "RII_BUDGET_PlanMonthlyProjection",
                newName: "IX_RII_BUDGET_PlanMonthlyProjection_UpdatedBy");

            migrationBuilder.RenameIndex(
                name: "IX_RII_BUDGET_PLAN_MONTHLY_PROJECTION_DeletedBy",
                table: "RII_BUDGET_PlanMonthlyProjection",
                newName: "IX_RII_BUDGET_PlanMonthlyProjection_DeletedBy");

            migrationBuilder.RenameIndex(
                name: "IX_RII_BUDGET_PLAN_MONTHLY_PROJECTION_CreatedBy",
                table: "RII_BUDGET_PlanMonthlyProjection",
                newName: "IX_RII_BUDGET_PlanMonthlyProjection_CreatedBy");

            migrationBuilder.RenameIndex(
                name: "IX_RII_BUDGET_PLAN_MONTHLY_PROJECTION_CalibrationDefinitionId",
                table: "RII_BUDGET_PlanMonthlyProjection",
                newName: "IX_RII_BUDGET_PlanMonthlyProjection_CalibrationDefinitionId");

            migrationBuilder.RenameIndex(
                name: "IX_RII_BUDGET_PLAN_MONTHLY_PROJECTION_BudgetPlanFishBatchId",
                table: "RII_BUDGET_PlanMonthlyProjection",
                newName: "IX_RII_BUDGET_PlanMonthlyProjection_BudgetPlanFishBatchId");

            migrationBuilder.RenameIndex(
                name: "IX_RII_BUDGET_PLAN_FISH_BATCH_ADJUSTMENT_UpdatedBy",
                table: "RII_BUDGET_PlanFishBatchAdjustment",
                newName: "IX_RII_BUDGET_PlanFishBatchAdjustment_UpdatedBy");

            migrationBuilder.RenameIndex(
                name: "IX_RII_BUDGET_PLAN_FISH_BATCH_ADJUSTMENT_DeletedBy",
                table: "RII_BUDGET_PlanFishBatchAdjustment",
                newName: "IX_RII_BUDGET_PlanFishBatchAdjustment_DeletedBy");

            migrationBuilder.RenameIndex(
                name: "IX_RII_BUDGET_PLAN_FISH_BATCH_ADJUSTMENT_CreatedBy",
                table: "RII_BUDGET_PlanFishBatchAdjustment",
                newName: "IX_RII_BUDGET_PlanFishBatchAdjustment_CreatedBy");

            migrationBuilder.RenameIndex(
                name: "IX_RII_BUDGET_PLAN_FISH_BATCH_ADJUSTMENT_BudgetPlanFishBatchId",
                table: "RII_BUDGET_PlanFishBatchAdjustment",
                newName: "IX_RII_BUDGET_PlanFishBatchAdjustment_BudgetPlanFishBatchId");

            migrationBuilder.RenameIndex(
                name: "IX_RII_BUDGET_PLAN_FISH_BATCH_ADJUSTMENT_Batch",
                table: "RII_BUDGET_PlanFishBatchAdjustment",
                newName: "IX_RII_BUDGET_PlanFishBatchAdjustment_Batch");

            migrationBuilder.RenameIndex(
                name: "IX_RII_BUDGET_PLAN_FISH_BATCH_UpdatedBy",
                table: "RII_BUDGET_PlanFishBatch",
                newName: "IX_RII_BUDGET_PlanFishBatch_UpdatedBy");

            migrationBuilder.RenameIndex(
                name: "IX_RII_BUDGET_PLAN_FISH_BATCH_SourceFishBatchId",
                table: "RII_BUDGET_PlanFishBatch",
                newName: "IX_RII_BUDGET_PlanFishBatch_SourceFishBatchId");

            migrationBuilder.RenameIndex(
                name: "IX_RII_BUDGET_PLAN_FISH_BATCH_FishStockId",
                table: "RII_BUDGET_PlanFishBatch",
                newName: "IX_RII_BUDGET_PlanFishBatch_FishStockId");

            migrationBuilder.RenameIndex(
                name: "IX_RII_BUDGET_PLAN_FISH_BATCH_DeletedBy",
                table: "RII_BUDGET_PlanFishBatch",
                newName: "IX_RII_BUDGET_PlanFishBatch_DeletedBy");

            migrationBuilder.RenameIndex(
                name: "IX_RII_BUDGET_PLAN_FISH_BATCH_CreatedBy",
                table: "RII_BUDGET_PlanFishBatch",
                newName: "IX_RII_BUDGET_PlanFishBatch_CreatedBy");

            migrationBuilder.RenameIndex(
                name: "IX_RII_BUDGET_PLAN_FISH_BATCH_BudgetPlanProjectId",
                table: "RII_BUDGET_PlanFishBatch",
                newName: "IX_RII_BUDGET_PlanFishBatch_BudgetPlanProjectId");

            migrationBuilder.RenameIndex(
                name: "IX_RII_BUDGET_PLAN_FISH_BATCH_BatchCode_Active",
                table: "RII_BUDGET_PlanFishBatch",
                newName: "IX_RII_BUDGET_PlanFishBatch_BatchCode_Active");

            migrationBuilder.RenameIndex(
                name: "IX_RII_BUDGET_PLAN_FEEDING_LINE_UpdatedBy",
                table: "RII_BUDGET_PlanFeedingLine",
                newName: "IX_RII_BUDGET_PlanFeedingLine_UpdatedBy");

            migrationBuilder.RenameIndex(
                name: "IX_RII_BUDGET_PLAN_FEEDING_LINE_FeedStockId",
                table: "RII_BUDGET_PlanFeedingLine",
                newName: "IX_RII_BUDGET_PlanFeedingLine_FeedStockId");

            migrationBuilder.RenameIndex(
                name: "IX_RII_BUDGET_PLAN_FEEDING_LINE_DeletedBy",
                table: "RII_BUDGET_PlanFeedingLine",
                newName: "IX_RII_BUDGET_PlanFeedingLine_DeletedBy");

            migrationBuilder.RenameIndex(
                name: "IX_RII_BUDGET_PLAN_FEEDING_LINE_CreatedBy",
                table: "RII_BUDGET_PlanFeedingLine",
                newName: "IX_RII_BUDGET_PlanFeedingLine_CreatedBy");

            migrationBuilder.RenameIndex(
                name: "IX_RII_BUDGET_PLAN_FEEDING_LINE_BudgetPlanMonthlyProjectionId",
                table: "RII_BUDGET_PlanFeedingLine",
                newName: "IX_RII_BUDGET_PlanFeedingLine_BudgetPlanMonthlyProjectionId");

            migrationBuilder.RenameIndex(
                name: "IX_RII_BUDGET_PLAN_FEEDING_LINE_BudgetPlanId",
                table: "RII_BUDGET_PlanFeedingLine",
                newName: "IX_RII_BUDGET_PlanFeedingLine_BudgetPlanId");

            migrationBuilder.RenameIndex(
                name: "IX_RII_BUDGET_PLAN_FEEDING_LINE_BudgetPlanFishBatchId",
                table: "RII_BUDGET_PlanFeedingLine",
                newName: "IX_RII_BUDGET_PlanFeedingLine_BudgetPlanFishBatchId");

            migrationBuilder.RenameIndex(
                name: "UX_RII_BUDGET_PLAN_EXCHANGE_RATE_PeriodCurrency",
                table: "RII_BUDGET_PlanExchangeRate",
                newName: "UX_RII_BUDGET_PlanExchangeRate_PeriodCurrency");

            migrationBuilder.RenameIndex(
                name: "IX_RII_BUDGET_PLAN_EXCHANGE_RATE_UpdatedBy",
                table: "RII_BUDGET_PlanExchangeRate",
                newName: "IX_RII_BUDGET_PlanExchangeRate_UpdatedBy");

            migrationBuilder.RenameIndex(
                name: "IX_RII_BUDGET_PLAN_EXCHANGE_RATE_DeletedBy",
                table: "RII_BUDGET_PlanExchangeRate",
                newName: "IX_RII_BUDGET_PlanExchangeRate_DeletedBy");

            migrationBuilder.RenameIndex(
                name: "IX_RII_BUDGET_PLAN_EXCHANGE_RATE_CreatedBy",
                table: "RII_BUDGET_PlanExchangeRate",
                newName: "IX_RII_BUDGET_PlanExchangeRate_CreatedBy");

            migrationBuilder.RenameIndex(
                name: "IX_RII_BUDGET_MORTALITY_RATE_DEFINITION_UpdatedBy",
                table: "RII_BUDGET_MortalityRateDefinition",
                newName: "IX_RII_BUDGET_MortalityRateDefinition_UpdatedBy");

            migrationBuilder.RenameIndex(
                name: "IX_RII_BUDGET_MORTALITY_RATE_DEFINITION_Key_Active",
                table: "RII_BUDGET_MortalityRateDefinition",
                newName: "IX_RII_BUDGET_MortalityRateDefinition_Key_Active");

            migrationBuilder.RenameIndex(
                name: "IX_RII_BUDGET_MORTALITY_RATE_DEFINITION_DeletedBy",
                table: "RII_BUDGET_MortalityRateDefinition",
                newName: "IX_RII_BUDGET_MortalityRateDefinition_DeletedBy");

            migrationBuilder.RenameIndex(
                name: "IX_RII_BUDGET_MORTALITY_RATE_DEFINITION_CreatedBy",
                table: "RII_BUDGET_MortalityRateDefinition",
                newName: "IX_RII_BUDGET_MortalityRateDefinition_CreatedBy");

            migrationBuilder.RenameIndex(
                name: "IX_RII_BUDGET_MORTALITY_RATE_DEFINITION_CalibrationDefinitionId",
                table: "RII_BUDGET_MortalityRateDefinition",
                newName: "IX_RII_BUDGET_MortalityRateDefinition_CalibrationDefinitionId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RII_BUDGET_Plan",
                table: "RII_BUDGET_Plan",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RII_BUDGET_PlanSalesLine",
                table: "RII_BUDGET_PlanSalesLine",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RII_BUDGET_PlanProject",
                table: "RII_BUDGET_PlanProject",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RII_BUDGET_PlanMortalityLine",
                table: "RII_BUDGET_PlanMortalityLine",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RII_BUDGET_PlanMonthlyProjection",
                table: "RII_BUDGET_PlanMonthlyProjection",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RII_BUDGET_PlanFishBatchAdjustment",
                table: "RII_BUDGET_PlanFishBatchAdjustment",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RII_BUDGET_PlanFishBatch",
                table: "RII_BUDGET_PlanFishBatch",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RII_BUDGET_PlanFeedingLine",
                table: "RII_BUDGET_PlanFeedingLine",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RII_BUDGET_PlanExchangeRate",
                table: "RII_BUDGET_PlanExchangeRate",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RII_BUDGET_MortalityRateDefinition",
                table: "RII_BUDGET_MortalityRateDefinition",
                column: "Id");

            migrationBuilder.AddCheckConstraint(
                name: "CK_RII_BUDGET_PlanSalesLine_Month",
                table: "RII_BUDGET_PlanSalesLine",
                sql: "[Month] BETWEEN 1 AND 12");

            migrationBuilder.AddCheckConstraint(
                name: "CK_RII_BUDGET_PlanSalesLine_NonNegative",
                table: "RII_BUDGET_PlanSalesLine",
                sql: "[SalesKg] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_RII_BUDGET_PlanProject_SourceType",
                table: "RII_BUDGET_PlanProject",
                sql: "[SourceType] IN (0,1)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_RII_BUDGET_PlanMortalityLine_Month",
                table: "RII_BUDGET_PlanMortalityLine",
                sql: "[Month] BETWEEN 1 AND 12");

            migrationBuilder.AddCheckConstraint(
                name: "CK_RII_BUDGET_PlanMortalityLine_NonNegative",
                table: "RII_BUDGET_PlanMortalityLine",
                sql: "[MortalityRatePercent] >= 0 AND [MortalityCount] >= 0 AND [MortalityKg] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_RII_BUDGET_PlanMonthlyProjection_Month",
                table: "RII_BUDGET_PlanMonthlyProjection",
                sql: "[Month] BETWEEN 1 AND 12");

            migrationBuilder.AddCheckConstraint(
                name: "CK_RII_BUDGET_PlanMonthlyProjection_NonNegative",
                table: "RII_BUDGET_PlanMonthlyProjection",
                sql: "[OpeningLiveCount] >= 0 AND [ClosingLiveCount] >= 0 AND [OpeningBiomassKg] >= 0 AND [ClosingBiomassKg] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_RII_BUDGET_PlanFishBatchAdjustment_Positive",
                table: "RII_BUDGET_PlanFishBatchAdjustment",
                sql: "[LiveCount] > 0 AND [AverageGram] >= 0 AND [BiomassKg] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_RII_BUDGET_PlanFishBatchAdjustment_Type",
                table: "RII_BUDGET_PlanFishBatchAdjustment",
                sql: "[AdjustmentType] IN (0,1,2,3)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_RII_BUDGET_PlanFishBatch_GrowthStartMonth",
                table: "RII_BUDGET_PlanFishBatch",
                sql: "[GrowthStartMonth] BETWEEN 1 AND 12");

            migrationBuilder.AddCheckConstraint(
                name: "CK_RII_BUDGET_PlanFishBatch_NonNegative",
                table: "RII_BUDGET_PlanFishBatch",
                sql: "[InitialLiveCount] >= 0 AND [InitialAverageGram] >= 0 AND [InitialBiomassKg] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_RII_BUDGET_PlanFishBatch_SourceType",
                table: "RII_BUDGET_PlanFishBatch",
                sql: "[SourceType] IN (0,1)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_RII_BUDGET_PlanFeedingLine_Month",
                table: "RII_BUDGET_PlanFeedingLine",
                sql: "[Month] BETWEEN 1 AND 12");

            migrationBuilder.AddCheckConstraint(
                name: "CK_RII_BUDGET_PlanFeedingLine_NonNegative",
                table: "RII_BUDGET_PlanFeedingLine",
                sql: "[FeedAmountRate] >= 0 AND [FeedKg] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_RII_BUDGET_PlanExchangeRate_Month",
                table: "RII_BUDGET_PlanExchangeRate",
                sql: "[Month] BETWEEN 1 AND 12");

            migrationBuilder.AddCheckConstraint(
                name: "CK_RII_BUDGET_PlanExchangeRate_Rate",
                table: "RII_BUDGET_PlanExchangeRate",
                sql: "[ExchangeRate] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_RII_BUDGET_MortalityRateDefinition_Rate",
                table: "RII_BUDGET_MortalityRateDefinition",
                sql: "[MortalityRatePercent] >= 0 AND [MortalityRatePercent] <= 100");

            migrationBuilder.AddForeignKey(
                name: "FK_RII_BUDGET_MortalityRateDefinition_RII_BUDGET_CALIBRATION_DEFINITION_CalibrationDefinitionId",
                table: "RII_BUDGET_MortalityRateDefinition",
                column: "CalibrationDefinitionId",
                principalTable: "RII_BUDGET_CALIBRATION_DEFINITION",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RII_BUDGET_MortalityRateDefinition_RII_STOCK_FishStockId",
                table: "RII_BUDGET_MortalityRateDefinition",
                column: "FishStockId",
                principalTable: "RII_STOCK",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RII_BUDGET_MortalityRateDefinition_RII_USERS_CreatedBy",
                table: "RII_BUDGET_MortalityRateDefinition",
                column: "CreatedBy",
                principalTable: "RII_USERS",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RII_BUDGET_MortalityRateDefinition_RII_USERS_DeletedBy",
                table: "RII_BUDGET_MortalityRateDefinition",
                column: "DeletedBy",
                principalTable: "RII_USERS",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RII_BUDGET_MortalityRateDefinition_RII_USERS_UpdatedBy",
                table: "RII_BUDGET_MortalityRateDefinition",
                column: "UpdatedBy",
                principalTable: "RII_USERS",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RII_BUDGET_Plan_RII_USERS_CreatedBy",
                table: "RII_BUDGET_Plan",
                column: "CreatedBy",
                principalTable: "RII_USERS",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RII_BUDGET_Plan_RII_USERS_DeletedBy",
                table: "RII_BUDGET_Plan",
                column: "DeletedBy",
                principalTable: "RII_USERS",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RII_BUDGET_Plan_RII_USERS_UpdatedBy",
                table: "RII_BUDGET_Plan",
                column: "UpdatedBy",
                principalTable: "RII_USERS",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RII_BUDGET_PlanExchangeRate_RII_BUDGET_Plan_BudgetPlanId",
                table: "RII_BUDGET_PlanExchangeRate",
                column: "BudgetPlanId",
                principalTable: "RII_BUDGET_Plan",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RII_BUDGET_PlanExchangeRate_RII_USERS_CreatedBy",
                table: "RII_BUDGET_PlanExchangeRate",
                column: "CreatedBy",
                principalTable: "RII_USERS",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RII_BUDGET_PlanExchangeRate_RII_USERS_DeletedBy",
                table: "RII_BUDGET_PlanExchangeRate",
                column: "DeletedBy",
                principalTable: "RII_USERS",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RII_BUDGET_PlanExchangeRate_RII_USERS_UpdatedBy",
                table: "RII_BUDGET_PlanExchangeRate",
                column: "UpdatedBy",
                principalTable: "RII_USERS",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RII_BUDGET_PlanFeedingLine_RII_BUDGET_PlanFishBatch_BudgetPlanFishBatchId",
                table: "RII_BUDGET_PlanFeedingLine",
                column: "BudgetPlanFishBatchId",
                principalTable: "RII_BUDGET_PlanFishBatch",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RII_BUDGET_PlanFeedingLine_RII_BUDGET_PlanMonthlyProjection_BudgetPlanMonthlyProjectionId",
                table: "RII_BUDGET_PlanFeedingLine",
                column: "BudgetPlanMonthlyProjectionId",
                principalTable: "RII_BUDGET_PlanMonthlyProjection",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RII_BUDGET_PlanFeedingLine_RII_BUDGET_Plan_BudgetPlanId",
                table: "RII_BUDGET_PlanFeedingLine",
                column: "BudgetPlanId",
                principalTable: "RII_BUDGET_Plan",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RII_BUDGET_PlanFeedingLine_RII_STOCK_FeedStockId",
                table: "RII_BUDGET_PlanFeedingLine",
                column: "FeedStockId",
                principalTable: "RII_STOCK",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RII_BUDGET_PlanFeedingLine_RII_USERS_CreatedBy",
                table: "RII_BUDGET_PlanFeedingLine",
                column: "CreatedBy",
                principalTable: "RII_USERS",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RII_BUDGET_PlanFeedingLine_RII_USERS_DeletedBy",
                table: "RII_BUDGET_PlanFeedingLine",
                column: "DeletedBy",
                principalTable: "RII_USERS",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RII_BUDGET_PlanFeedingLine_RII_USERS_UpdatedBy",
                table: "RII_BUDGET_PlanFeedingLine",
                column: "UpdatedBy",
                principalTable: "RII_USERS",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RII_BUDGET_PlanFishBatch_RII_BUDGET_PlanProject_BudgetPlanProjectId",
                table: "RII_BUDGET_PlanFishBatch",
                column: "BudgetPlanProjectId",
                principalTable: "RII_BUDGET_PlanProject",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RII_BUDGET_PlanFishBatch_RII_BUDGET_Plan_BudgetPlanId",
                table: "RII_BUDGET_PlanFishBatch",
                column: "BudgetPlanId",
                principalTable: "RII_BUDGET_Plan",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RII_BUDGET_PlanFishBatch_RII_FishBatch_SourceFishBatchId",
                table: "RII_BUDGET_PlanFishBatch",
                column: "SourceFishBatchId",
                principalTable: "RII_FishBatch",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RII_BUDGET_PlanFishBatch_RII_STOCK_FishStockId",
                table: "RII_BUDGET_PlanFishBatch",
                column: "FishStockId",
                principalTable: "RII_STOCK",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RII_BUDGET_PlanFishBatch_RII_USERS_CreatedBy",
                table: "RII_BUDGET_PlanFishBatch",
                column: "CreatedBy",
                principalTable: "RII_USERS",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RII_BUDGET_PlanFishBatch_RII_USERS_DeletedBy",
                table: "RII_BUDGET_PlanFishBatch",
                column: "DeletedBy",
                principalTable: "RII_USERS",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RII_BUDGET_PlanFishBatch_RII_USERS_UpdatedBy",
                table: "RII_BUDGET_PlanFishBatch",
                column: "UpdatedBy",
                principalTable: "RII_USERS",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RII_BUDGET_PlanFishBatchAdjustment_RII_BUDGET_PlanFishBatch_BudgetPlanFishBatchId",
                table: "RII_BUDGET_PlanFishBatchAdjustment",
                column: "BudgetPlanFishBatchId",
                principalTable: "RII_BUDGET_PlanFishBatch",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RII_BUDGET_PlanFishBatchAdjustment_RII_BUDGET_Plan_BudgetPlanId",
                table: "RII_BUDGET_PlanFishBatchAdjustment",
                column: "BudgetPlanId",
                principalTable: "RII_BUDGET_Plan",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RII_BUDGET_PlanFishBatchAdjustment_RII_USERS_CreatedBy",
                table: "RII_BUDGET_PlanFishBatchAdjustment",
                column: "CreatedBy",
                principalTable: "RII_USERS",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RII_BUDGET_PlanFishBatchAdjustment_RII_USERS_DeletedBy",
                table: "RII_BUDGET_PlanFishBatchAdjustment",
                column: "DeletedBy",
                principalTable: "RII_USERS",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RII_BUDGET_PlanFishBatchAdjustment_RII_USERS_UpdatedBy",
                table: "RII_BUDGET_PlanFishBatchAdjustment",
                column: "UpdatedBy",
                principalTable: "RII_USERS",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RII_BUDGET_PlanMonthlyProjection_RII_BUDGET_CALIBRATION_DEFINITION_CalibrationDefinitionId",
                table: "RII_BUDGET_PlanMonthlyProjection",
                column: "CalibrationDefinitionId",
                principalTable: "RII_BUDGET_CALIBRATION_DEFINITION",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RII_BUDGET_PlanMonthlyProjection_RII_BUDGET_PlanFishBatch_BudgetPlanFishBatchId",
                table: "RII_BUDGET_PlanMonthlyProjection",
                column: "BudgetPlanFishBatchId",
                principalTable: "RII_BUDGET_PlanFishBatch",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RII_BUDGET_PlanMonthlyProjection_RII_BUDGET_Plan_BudgetPlanId",
                table: "RII_BUDGET_PlanMonthlyProjection",
                column: "BudgetPlanId",
                principalTable: "RII_BUDGET_Plan",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RII_BUDGET_PlanMonthlyProjection_RII_BUDGET_WATER_TEMPERATURE_WaterTemperatureId",
                table: "RII_BUDGET_PlanMonthlyProjection",
                column: "WaterTemperatureId",
                principalTable: "RII_BUDGET_WATER_TEMPERATURE",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RII_BUDGET_PlanMonthlyProjection_RII_USERS_CreatedBy",
                table: "RII_BUDGET_PlanMonthlyProjection",
                column: "CreatedBy",
                principalTable: "RII_USERS",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RII_BUDGET_PlanMonthlyProjection_RII_USERS_DeletedBy",
                table: "RII_BUDGET_PlanMonthlyProjection",
                column: "DeletedBy",
                principalTable: "RII_USERS",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RII_BUDGET_PlanMonthlyProjection_RII_USERS_UpdatedBy",
                table: "RII_BUDGET_PlanMonthlyProjection",
                column: "UpdatedBy",
                principalTable: "RII_USERS",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RII_BUDGET_PlanMortalityLine_RII_BUDGET_PlanFishBatch_BudgetPlanFishBatchId",
                table: "RII_BUDGET_PlanMortalityLine",
                column: "BudgetPlanFishBatchId",
                principalTable: "RII_BUDGET_PlanFishBatch",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RII_BUDGET_PlanMortalityLine_RII_BUDGET_PlanMonthlyProjection_BudgetPlanMonthlyProjectionId",
                table: "RII_BUDGET_PlanMortalityLine",
                column: "BudgetPlanMonthlyProjectionId",
                principalTable: "RII_BUDGET_PlanMonthlyProjection",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RII_BUDGET_PlanMortalityLine_RII_BUDGET_Plan_BudgetPlanId",
                table: "RII_BUDGET_PlanMortalityLine",
                column: "BudgetPlanId",
                principalTable: "RII_BUDGET_Plan",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RII_BUDGET_PlanMortalityLine_RII_USERS_CreatedBy",
                table: "RII_BUDGET_PlanMortalityLine",
                column: "CreatedBy",
                principalTable: "RII_USERS",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RII_BUDGET_PlanMortalityLine_RII_USERS_DeletedBy",
                table: "RII_BUDGET_PlanMortalityLine",
                column: "DeletedBy",
                principalTable: "RII_USERS",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RII_BUDGET_PlanMortalityLine_RII_USERS_UpdatedBy",
                table: "RII_BUDGET_PlanMortalityLine",
                column: "UpdatedBy",
                principalTable: "RII_USERS",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RII_BUDGET_PlanProject_RII_BUDGET_Plan_BudgetPlanId",
                table: "RII_BUDGET_PlanProject",
                column: "BudgetPlanId",
                principalTable: "RII_BUDGET_Plan",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RII_BUDGET_PlanProject_RII_Project_SourceProjectId",
                table: "RII_BUDGET_PlanProject",
                column: "SourceProjectId",
                principalTable: "RII_Project",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RII_BUDGET_PlanProject_RII_USERS_CreatedBy",
                table: "RII_BUDGET_PlanProject",
                column: "CreatedBy",
                principalTable: "RII_USERS",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RII_BUDGET_PlanProject_RII_USERS_DeletedBy",
                table: "RII_BUDGET_PlanProject",
                column: "DeletedBy",
                principalTable: "RII_USERS",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RII_BUDGET_PlanProject_RII_USERS_UpdatedBy",
                table: "RII_BUDGET_PlanProject",
                column: "UpdatedBy",
                principalTable: "RII_USERS",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RII_BUDGET_PlanSalesLine_RII_BUDGET_PlanFishBatch_BudgetPlanFishBatchId",
                table: "RII_BUDGET_PlanSalesLine",
                column: "BudgetPlanFishBatchId",
                principalTable: "RII_BUDGET_PlanFishBatch",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RII_BUDGET_PlanSalesLine_RII_BUDGET_Plan_BudgetPlanId",
                table: "RII_BUDGET_PlanSalesLine",
                column: "BudgetPlanId",
                principalTable: "RII_BUDGET_Plan",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RII_BUDGET_PlanSalesLine_RII_USERS_CreatedBy",
                table: "RII_BUDGET_PlanSalesLine",
                column: "CreatedBy",
                principalTable: "RII_USERS",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RII_BUDGET_PlanSalesLine_RII_USERS_DeletedBy",
                table: "RII_BUDGET_PlanSalesLine",
                column: "DeletedBy",
                principalTable: "RII_USERS",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RII_BUDGET_PlanSalesLine_RII_USERS_UpdatedBy",
                table: "RII_BUDGET_PlanSalesLine",
                column: "UpdatedBy",
                principalTable: "RII_USERS",
                principalColumn: "Id");
        }
    }
}
