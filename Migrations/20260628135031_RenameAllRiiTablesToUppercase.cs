using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace aqua_api.Migrations
{
    /// <inheritdoc />
    public partial class RenameAllRiiTablesToUppercase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_WelfareAssessment]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_WELFARE_ASSESSMENT]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_WelfareAssessment]', N'RII_WELFARE_ASSESSMENT';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_WeighingLine]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_WEIGHING_LINE]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_WeighingLine]', N'RII_WEIGHING_LINE';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_WeatherType]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_WEATHER_TYPE]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_WeatherType]', N'RII_WEATHER_TYPE';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_WeatherSeverity]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_WEATHER_SEVERITY]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_WeatherSeverity]', N'RII_WEATHER_SEVERITY';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_WarehouseTransferLine]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_WAREHOUSE_TRANSFER_LINE]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_WarehouseTransferLine]', N'RII_WAREHOUSE_TRANSFER_LINE';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_WarehouseTransfer]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_WAREHOUSE_TRANSFER]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_WarehouseTransfer]', N'RII_WAREHOUSE_TRANSFER';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_WarehouseCageTransferLine]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_WAREHOUSE_CAGE_TRANSFER_LINE]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_WarehouseCageTransferLine]', N'RII_WAREHOUSE_CAGE_TRANSFER_LINE';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_WarehouseCageTransfer]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_WAREHOUSE_CAGE_TRANSFER]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_WarehouseCageTransfer]', N'RII_WAREHOUSE_CAGE_TRANSFER';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_TransferLine]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_TRANSFER_LINE]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_TransferLine]', N'RII_TRANSFER_LINE';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_StockConvertLine]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_STOCK_CONVERT_LINE]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_StockConvertLine]', N'RII_STOCK_CONVERT_LINE';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_StockConvert]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_STOCK_CONVERT]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_StockConvert]', N'RII_STOCK_CONVERT';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_ShipmentLine]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_SHIPMENT_LINE]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_ShipmentLine]', N'RII_SHIPMENT_LINE';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_ProjectMergeSource]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_PROJECT_MERGE_SOURCE]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_ProjectMergeSource]', N'RII_PROJECT_MERGE_SOURCE';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_ProjectMergeCage]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_PROJECT_MERGE_CAGE]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_ProjectMergeCage]', N'RII_PROJECT_MERGE_CAGE';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_ProjectMerge]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_PROJECT_MERGE]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_ProjectMerge]', N'RII_PROJECT_MERGE';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_ProjectCageDailyKpiSnapshot]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_PROJECT_CAGE_DAILY_KPI_SNAPSHOT]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_ProjectCageDailyKpiSnapshot]', N'RII_PROJECT_CAGE_DAILY_KPI_SNAPSHOT';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_ProjectCage]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_PROJECT_CAGE]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_ProjectCage]', N'RII_PROJECT_CAGE';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_OpeningImportRow]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_OPENING_IMPORT_ROW]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_OpeningImportRow]', N'RII_OPENING_IMPORT_ROW';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_OpeningImportJob]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_OPENING_IMPORT_JOB]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_OpeningImportJob]', N'RII_OPENING_IMPORT_JOB';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_NetOperationType]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_NET_OPERATION_TYPE]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_NetOperationType]', N'RII_NET_OPERATION_TYPE';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_NetOperationLine]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_NET_OPERATION_LINE]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_NetOperationLine]', N'RII_NET_OPERATION_LINE';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_NetOperation]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_NET_OPERATION]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_NetOperation]', N'RII_NET_OPERATION';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_NetInventoryMovement]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_NET_INVENTORY_MOVEMENT]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_NetInventoryMovement]', N'RII_NET_INVENTORY_MOVEMENT';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_MortalityLine]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_MORTALITY_LINE]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_MortalityLine]', N'RII_MORTALITY_LINE';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_GoodsReceiptLine]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_GOODS_RECEIPT_LINE]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_GoodsReceiptLine]', N'RII_GOODS_RECEIPT_LINE';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_GoodsReceiptFishDistribution]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_GOODS_RECEIPT_FISH_DISTRIBUTION]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_GoodsReceiptFishDistribution]', N'RII_GOODS_RECEIPT_FISH_DISTRIBUTION';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_GoodsReceipt]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_GOODS_RECEIPT]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_GoodsReceipt]', N'RII_GOODS_RECEIPT';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_FishTreatment]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_FISH_TREATMENT]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_FishTreatment]', N'RII_FISH_TREATMENT';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_FishLabSample]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_FISH_LAB_SAMPLE]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_FishLabSample]', N'RII_FISH_LAB_SAMPLE';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_FishLabResult]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_FISH_LAB_RESULT]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_FishLabResult]', N'RII_FISH_LAB_RESULT';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_FishHealthEvent]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_FISH_HEALTH_EVENT]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_FishHealthEvent]', N'RII_FISH_HEALTH_EVENT';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_FishBatch]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_FISH_BATCH]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_FishBatch]', N'RII_FISH_BATCH';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_FeedingLine]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_FEEDING_LINE]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_FeedingLine]', N'RII_FEEDING_LINE';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_FeedingDistribution]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_FEEDING_DISTRIBUTION]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_FeedingDistribution]', N'RII_FEEDING_DISTRIBUTION';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_DailyWeather]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_DAILY_WEATHER]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_DailyWeather]', N'RII_DAILY_WEATHER';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_ComplianceCorrectiveAction]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_COMPLIANCE_CORRECTIVE_ACTION]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_ComplianceCorrectiveAction]', N'RII_COMPLIANCE_CORRECTIVE_ACTION';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_ComplianceAudit]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_COMPLIANCE_AUDIT]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_ComplianceAudit]', N'RII_COMPLIANCE_AUDIT';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_CageWarehouseTransferLine]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_CAGE_WAREHOUSE_TRANSFER_LINE]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_CageWarehouseTransferLine]', N'RII_CAGE_WAREHOUSE_TRANSFER_LINE';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_CageWarehouseTransfer]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_CAGE_WAREHOUSE_TRANSFER]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_CageWarehouseTransfer]', N'RII_CAGE_WAREHOUSE_TRANSFER';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_CageWarehouseMapping]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_CAGE_WAREHOUSE_MAPPING]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_CageWarehouseMapping]', N'RII_CAGE_WAREHOUSE_MAPPING';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_BatchWarehouseBalance]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_BATCH_WAREHOUSE_BALANCE]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_BatchWarehouseBalance]', N'RII_BATCH_WAREHOUSE_BALANCE';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_BatchMovement]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_BATCH_MOVEMENT]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_BatchMovement]', N'RII_BATCH_MOVEMENT';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_BatchCageBalance]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_BATCH_CAGE_BALANCE]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_BatchCageBalance]', N'RII_BATCH_CAGE_BALANCE';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_AquaSetting]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_AQUA_SETTING]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_AquaSetting]', N'RII_AQUA_SETTING';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_WELFARE_ASSESSMENT]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_WelfareAssessment]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_WELFARE_ASSESSMENT]', N'RII_WelfareAssessment';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_WEIGHING_LINE]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_WeighingLine]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_WEIGHING_LINE]', N'RII_WeighingLine';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_WEATHER_TYPE]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_WeatherType]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_WEATHER_TYPE]', N'RII_WeatherType';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_WEATHER_SEVERITY]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_WeatherSeverity]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_WEATHER_SEVERITY]', N'RII_WeatherSeverity';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_WAREHOUSE_TRANSFER_LINE]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_WarehouseTransferLine]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_WAREHOUSE_TRANSFER_LINE]', N'RII_WarehouseTransferLine';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_WAREHOUSE_TRANSFER]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_WarehouseTransfer]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_WAREHOUSE_TRANSFER]', N'RII_WarehouseTransfer';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_WAREHOUSE_CAGE_TRANSFER_LINE]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_WarehouseCageTransferLine]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_WAREHOUSE_CAGE_TRANSFER_LINE]', N'RII_WarehouseCageTransferLine';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_WAREHOUSE_CAGE_TRANSFER]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_WarehouseCageTransfer]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_WAREHOUSE_CAGE_TRANSFER]', N'RII_WarehouseCageTransfer';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_TRANSFER_LINE]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_TransferLine]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_TRANSFER_LINE]', N'RII_TransferLine';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_STOCK_CONVERT_LINE]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_StockConvertLine]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_STOCK_CONVERT_LINE]', N'RII_StockConvertLine';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_STOCK_CONVERT]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_StockConvert]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_STOCK_CONVERT]', N'RII_StockConvert';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_SHIPMENT_LINE]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_ShipmentLine]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_SHIPMENT_LINE]', N'RII_ShipmentLine';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_PROJECT_MERGE_SOURCE]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_ProjectMergeSource]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_PROJECT_MERGE_SOURCE]', N'RII_ProjectMergeSource';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_PROJECT_MERGE_CAGE]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_ProjectMergeCage]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_PROJECT_MERGE_CAGE]', N'RII_ProjectMergeCage';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_PROJECT_MERGE]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_ProjectMerge]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_PROJECT_MERGE]', N'RII_ProjectMerge';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_PROJECT_CAGE_DAILY_KPI_SNAPSHOT]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_ProjectCageDailyKpiSnapshot]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_PROJECT_CAGE_DAILY_KPI_SNAPSHOT]', N'RII_ProjectCageDailyKpiSnapshot';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_PROJECT_CAGE]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_ProjectCage]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_PROJECT_CAGE]', N'RII_ProjectCage';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_OPENING_IMPORT_ROW]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_OpeningImportRow]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_OPENING_IMPORT_ROW]', N'RII_OpeningImportRow';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_OPENING_IMPORT_JOB]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_OpeningImportJob]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_OPENING_IMPORT_JOB]', N'RII_OpeningImportJob';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_NET_OPERATION_TYPE]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_NetOperationType]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_NET_OPERATION_TYPE]', N'RII_NetOperationType';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_NET_OPERATION_LINE]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_NetOperationLine]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_NET_OPERATION_LINE]', N'RII_NetOperationLine';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_NET_OPERATION]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_NetOperation]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_NET_OPERATION]', N'RII_NetOperation';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_NET_INVENTORY_MOVEMENT]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_NetInventoryMovement]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_NET_INVENTORY_MOVEMENT]', N'RII_NetInventoryMovement';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_MORTALITY_LINE]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_MortalityLine]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_MORTALITY_LINE]', N'RII_MortalityLine';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_GOODS_RECEIPT_LINE]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_GoodsReceiptLine]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_GOODS_RECEIPT_LINE]', N'RII_GoodsReceiptLine';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_GOODS_RECEIPT_FISH_DISTRIBUTION]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_GoodsReceiptFishDistribution]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_GOODS_RECEIPT_FISH_DISTRIBUTION]', N'RII_GoodsReceiptFishDistribution';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_GOODS_RECEIPT]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_GoodsReceipt]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_GOODS_RECEIPT]', N'RII_GoodsReceipt';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_FISH_TREATMENT]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_FishTreatment]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_FISH_TREATMENT]', N'RII_FishTreatment';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_FISH_LAB_SAMPLE]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_FishLabSample]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_FISH_LAB_SAMPLE]', N'RII_FishLabSample';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_FISH_LAB_RESULT]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_FishLabResult]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_FISH_LAB_RESULT]', N'RII_FishLabResult';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_FISH_HEALTH_EVENT]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_FishHealthEvent]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_FISH_HEALTH_EVENT]', N'RII_FishHealthEvent';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_FISH_BATCH]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_FishBatch]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_FISH_BATCH]', N'RII_FishBatch';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_FEEDING_LINE]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_FeedingLine]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_FEEDING_LINE]', N'RII_FeedingLine';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_FEEDING_DISTRIBUTION]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_FeedingDistribution]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_FEEDING_DISTRIBUTION]', N'RII_FeedingDistribution';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_DAILY_WEATHER]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_DailyWeather]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_DAILY_WEATHER]', N'RII_DailyWeather';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_COMPLIANCE_CORRECTIVE_ACTION]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_ComplianceCorrectiveAction]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_COMPLIANCE_CORRECTIVE_ACTION]', N'RII_ComplianceCorrectiveAction';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_COMPLIANCE_AUDIT]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_ComplianceAudit]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_COMPLIANCE_AUDIT]', N'RII_ComplianceAudit';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_CAGE_WAREHOUSE_TRANSFER_LINE]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_CageWarehouseTransferLine]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_CAGE_WAREHOUSE_TRANSFER_LINE]', N'RII_CageWarehouseTransferLine';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_CAGE_WAREHOUSE_TRANSFER]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_CageWarehouseTransfer]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_CAGE_WAREHOUSE_TRANSFER]', N'RII_CageWarehouseTransfer';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_CAGE_WAREHOUSE_MAPPING]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_CageWarehouseMapping]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_CAGE_WAREHOUSE_MAPPING]', N'RII_CageWarehouseMapping';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_BATCH_WAREHOUSE_BALANCE]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_BatchWarehouseBalance]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_BATCH_WAREHOUSE_BALANCE]', N'RII_BatchWarehouseBalance';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_BATCH_MOVEMENT]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_BatchMovement]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_BATCH_MOVEMENT]', N'RII_BatchMovement';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_BATCH_CAGE_BALANCE]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_BatchCageBalance]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_BATCH_CAGE_BALANCE]', N'RII_BatchCageBalance';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_AQUA_SETTING]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_AquaSetting]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_AQUA_SETTING]', N'RII_AquaSetting';
END
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_WELFARE_ASSESSMENT]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_WelfareAssessment]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_WELFARE_ASSESSMENT]', N'RII_WelfareAssessment';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_WelfareAssessment]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_WELFARE_ASSESSMENT]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_WelfareAssessment]', N'RII_WELFARE_ASSESSMENT';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_WEIGHING_LINE]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_WeighingLine]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_WEIGHING_LINE]', N'RII_WeighingLine';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_WeighingLine]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_WEIGHING_LINE]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_WeighingLine]', N'RII_WEIGHING_LINE';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_WEATHER_TYPE]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_WeatherType]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_WEATHER_TYPE]', N'RII_WeatherType';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_WEATHER_SEVERITY]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_WeatherSeverity]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_WEATHER_SEVERITY]', N'RII_WeatherSeverity';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_WeatherType]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_WEATHER_TYPE]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_WeatherType]', N'RII_WEATHER_TYPE';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_WeatherSeverity]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_WEATHER_SEVERITY]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_WeatherSeverity]', N'RII_WEATHER_SEVERITY';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_WAREHOUSE_TRANSFER_LINE]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_WarehouseTransferLine]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_WAREHOUSE_TRANSFER_LINE]', N'RII_WarehouseTransferLine';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_WAREHOUSE_TRANSFER]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_WarehouseTransfer]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_WAREHOUSE_TRANSFER]', N'RII_WarehouseTransfer';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_WAREHOUSE_CAGE_TRANSFER_LINE]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_WarehouseCageTransferLine]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_WAREHOUSE_CAGE_TRANSFER_LINE]', N'RII_WarehouseCageTransferLine';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_WAREHOUSE_CAGE_TRANSFER]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_WarehouseCageTransfer]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_WAREHOUSE_CAGE_TRANSFER]', N'RII_WarehouseCageTransfer';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_WarehouseTransferLine]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_WAREHOUSE_TRANSFER_LINE]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_WarehouseTransferLine]', N'RII_WAREHOUSE_TRANSFER_LINE';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_WarehouseTransfer]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_WAREHOUSE_TRANSFER]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_WarehouseTransfer]', N'RII_WAREHOUSE_TRANSFER';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_WarehouseCageTransferLine]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_WAREHOUSE_CAGE_TRANSFER_LINE]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_WarehouseCageTransferLine]', N'RII_WAREHOUSE_CAGE_TRANSFER_LINE';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_WarehouseCageTransfer]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_WAREHOUSE_CAGE_TRANSFER]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_WarehouseCageTransfer]', N'RII_WAREHOUSE_CAGE_TRANSFER';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_TRANSFER_LINE]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_TransferLine]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_TRANSFER_LINE]', N'RII_TransferLine';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_TransferLine]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_TRANSFER_LINE]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_TransferLine]', N'RII_TRANSFER_LINE';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_STOCK_CONVERT_LINE]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_StockConvertLine]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_STOCK_CONVERT_LINE]', N'RII_StockConvertLine';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_STOCK_CONVERT]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_StockConvert]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_STOCK_CONVERT]', N'RII_StockConvert';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_StockConvertLine]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_STOCK_CONVERT_LINE]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_StockConvertLine]', N'RII_STOCK_CONVERT_LINE';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_StockConvert]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_STOCK_CONVERT]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_StockConvert]', N'RII_STOCK_CONVERT';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_SHIPMENT_LINE]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_ShipmentLine]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_SHIPMENT_LINE]', N'RII_ShipmentLine';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_ShipmentLine]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_SHIPMENT_LINE]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_ShipmentLine]', N'RII_SHIPMENT_LINE';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_PROJECT_MERGE_SOURCE]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_ProjectMergeSource]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_PROJECT_MERGE_SOURCE]', N'RII_ProjectMergeSource';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_PROJECT_MERGE_CAGE]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_ProjectMergeCage]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_PROJECT_MERGE_CAGE]', N'RII_ProjectMergeCage';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_PROJECT_MERGE]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_ProjectMerge]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_PROJECT_MERGE]', N'RII_ProjectMerge';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_PROJECT_CAGE_DAILY_KPI_SNAPSHOT]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_ProjectCageDailyKpiSnapshot]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_PROJECT_CAGE_DAILY_KPI_SNAPSHOT]', N'RII_ProjectCageDailyKpiSnapshot';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_PROJECT_CAGE]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_ProjectCage]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_PROJECT_CAGE]', N'RII_ProjectCage';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_ProjectMergeSource]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_PROJECT_MERGE_SOURCE]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_ProjectMergeSource]', N'RII_PROJECT_MERGE_SOURCE';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_ProjectMergeCage]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_PROJECT_MERGE_CAGE]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_ProjectMergeCage]', N'RII_PROJECT_MERGE_CAGE';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_ProjectMerge]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_PROJECT_MERGE]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_ProjectMerge]', N'RII_PROJECT_MERGE';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_ProjectCageDailyKpiSnapshot]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_PROJECT_CAGE_DAILY_KPI_SNAPSHOT]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_ProjectCageDailyKpiSnapshot]', N'RII_PROJECT_CAGE_DAILY_KPI_SNAPSHOT';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_ProjectCage]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_PROJECT_CAGE]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_ProjectCage]', N'RII_PROJECT_CAGE';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_OPENING_IMPORT_ROW]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_OpeningImportRow]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_OPENING_IMPORT_ROW]', N'RII_OpeningImportRow';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_OPENING_IMPORT_JOB]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_OpeningImportJob]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_OPENING_IMPORT_JOB]', N'RII_OpeningImportJob';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_OpeningImportRow]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_OPENING_IMPORT_ROW]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_OpeningImportRow]', N'RII_OPENING_IMPORT_ROW';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_OpeningImportJob]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_OPENING_IMPORT_JOB]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_OpeningImportJob]', N'RII_OPENING_IMPORT_JOB';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_NET_OPERATION_TYPE]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_NetOperationType]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_NET_OPERATION_TYPE]', N'RII_NetOperationType';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_NET_OPERATION_LINE]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_NetOperationLine]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_NET_OPERATION_LINE]', N'RII_NetOperationLine';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_NET_OPERATION]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_NetOperation]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_NET_OPERATION]', N'RII_NetOperation';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_NET_INVENTORY_MOVEMENT]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_NetInventoryMovement]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_NET_INVENTORY_MOVEMENT]', N'RII_NetInventoryMovement';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_NetOperationType]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_NET_OPERATION_TYPE]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_NetOperationType]', N'RII_NET_OPERATION_TYPE';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_NetOperationLine]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_NET_OPERATION_LINE]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_NetOperationLine]', N'RII_NET_OPERATION_LINE';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_NetOperation]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_NET_OPERATION]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_NetOperation]', N'RII_NET_OPERATION';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_NetInventoryMovement]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_NET_INVENTORY_MOVEMENT]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_NetInventoryMovement]', N'RII_NET_INVENTORY_MOVEMENT';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_MORTALITY_LINE]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_MortalityLine]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_MORTALITY_LINE]', N'RII_MortalityLine';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_MortalityLine]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_MORTALITY_LINE]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_MortalityLine]', N'RII_MORTALITY_LINE';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_GOODS_RECEIPT_LINE]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_GoodsReceiptLine]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_GOODS_RECEIPT_LINE]', N'RII_GoodsReceiptLine';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_GOODS_RECEIPT_FISH_DISTRIBUTION]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_GoodsReceiptFishDistribution]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_GOODS_RECEIPT_FISH_DISTRIBUTION]', N'RII_GoodsReceiptFishDistribution';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_GOODS_RECEIPT]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_GoodsReceipt]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_GOODS_RECEIPT]', N'RII_GoodsReceipt';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_GoodsReceiptLine]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_GOODS_RECEIPT_LINE]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_GoodsReceiptLine]', N'RII_GOODS_RECEIPT_LINE';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_GoodsReceiptFishDistribution]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_GOODS_RECEIPT_FISH_DISTRIBUTION]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_GoodsReceiptFishDistribution]', N'RII_GOODS_RECEIPT_FISH_DISTRIBUTION';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_GoodsReceipt]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_GOODS_RECEIPT]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_GoodsReceipt]', N'RII_GOODS_RECEIPT';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_FISH_TREATMENT]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_FishTreatment]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_FISH_TREATMENT]', N'RII_FishTreatment';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_FISH_LAB_SAMPLE]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_FishLabSample]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_FISH_LAB_SAMPLE]', N'RII_FishLabSample';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_FISH_LAB_RESULT]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_FishLabResult]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_FISH_LAB_RESULT]', N'RII_FishLabResult';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_FISH_HEALTH_EVENT]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_FishHealthEvent]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_FISH_HEALTH_EVENT]', N'RII_FishHealthEvent';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_FISH_BATCH]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_FishBatch]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_FISH_BATCH]', N'RII_FishBatch';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_FishTreatment]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_FISH_TREATMENT]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_FishTreatment]', N'RII_FISH_TREATMENT';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_FishLabSample]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_FISH_LAB_SAMPLE]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_FishLabSample]', N'RII_FISH_LAB_SAMPLE';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_FishLabResult]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_FISH_LAB_RESULT]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_FishLabResult]', N'RII_FISH_LAB_RESULT';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_FishHealthEvent]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_FISH_HEALTH_EVENT]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_FishHealthEvent]', N'RII_FISH_HEALTH_EVENT';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_FishBatch]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_FISH_BATCH]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_FishBatch]', N'RII_FISH_BATCH';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_FEEDING_LINE]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_FeedingLine]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_FEEDING_LINE]', N'RII_FeedingLine';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_FEEDING_DISTRIBUTION]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_FeedingDistribution]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_FEEDING_DISTRIBUTION]', N'RII_FeedingDistribution';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_FeedingLine]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_FEEDING_LINE]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_FeedingLine]', N'RII_FEEDING_LINE';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_FeedingDistribution]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_FEEDING_DISTRIBUTION]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_FeedingDistribution]', N'RII_FEEDING_DISTRIBUTION';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_DAILY_WEATHER]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_DailyWeather]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_DAILY_WEATHER]', N'RII_DailyWeather';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_DailyWeather]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_DAILY_WEATHER]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_DailyWeather]', N'RII_DAILY_WEATHER';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_COMPLIANCE_CORRECTIVE_ACTION]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_ComplianceCorrectiveAction]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_COMPLIANCE_CORRECTIVE_ACTION]', N'RII_ComplianceCorrectiveAction';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_COMPLIANCE_AUDIT]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_ComplianceAudit]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_COMPLIANCE_AUDIT]', N'RII_ComplianceAudit';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_ComplianceCorrectiveAction]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_COMPLIANCE_CORRECTIVE_ACTION]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_ComplianceCorrectiveAction]', N'RII_COMPLIANCE_CORRECTIVE_ACTION';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_ComplianceAudit]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_COMPLIANCE_AUDIT]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_ComplianceAudit]', N'RII_COMPLIANCE_AUDIT';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_CAGE_WAREHOUSE_TRANSFER_LINE]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_CageWarehouseTransferLine]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_CAGE_WAREHOUSE_TRANSFER_LINE]', N'RII_CageWarehouseTransferLine';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_CAGE_WAREHOUSE_TRANSFER]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_CageWarehouseTransfer]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_CAGE_WAREHOUSE_TRANSFER]', N'RII_CageWarehouseTransfer';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_CAGE_WAREHOUSE_MAPPING]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_CageWarehouseMapping]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_CAGE_WAREHOUSE_MAPPING]', N'RII_CageWarehouseMapping';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_CageWarehouseTransferLine]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_CAGE_WAREHOUSE_TRANSFER_LINE]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_CageWarehouseTransferLine]', N'RII_CAGE_WAREHOUSE_TRANSFER_LINE';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_CageWarehouseTransfer]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_CAGE_WAREHOUSE_TRANSFER]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_CageWarehouseTransfer]', N'RII_CAGE_WAREHOUSE_TRANSFER';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_CageWarehouseMapping]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_CAGE_WAREHOUSE_MAPPING]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_CageWarehouseMapping]', N'RII_CAGE_WAREHOUSE_MAPPING';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_BATCH_WAREHOUSE_BALANCE]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_BatchWarehouseBalance]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_BATCH_WAREHOUSE_BALANCE]', N'RII_BatchWarehouseBalance';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_BATCH_MOVEMENT]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_BatchMovement]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_BATCH_MOVEMENT]', N'RII_BatchMovement';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_BATCH_CAGE_BALANCE]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_BatchCageBalance]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_BATCH_CAGE_BALANCE]', N'RII_BatchCageBalance';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_BatchWarehouseBalance]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_BATCH_WAREHOUSE_BALANCE]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_BatchWarehouseBalance]', N'RII_BATCH_WAREHOUSE_BALANCE';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_BatchMovement]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_BATCH_MOVEMENT]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_BatchMovement]', N'RII_BATCH_MOVEMENT';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_BatchCageBalance]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_BATCH_CAGE_BALANCE]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_BatchCageBalance]', N'RII_BATCH_CAGE_BALANCE';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_AQUA_SETTING]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_AquaSetting]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_AQUA_SETTING]', N'RII_AquaSetting';
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[RII_AquaSetting]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_AQUA_SETTING]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[RII_AquaSetting]', N'RII_AQUA_SETTING';
END
");
        }
    }
}