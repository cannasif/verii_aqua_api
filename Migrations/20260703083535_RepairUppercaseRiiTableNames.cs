using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace aqua_api.Migrations
{
    /// <inheritdoc />
    public partial class RepairUppercaseRiiTableNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            RenameTablesToUppercase(migrationBuilder);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Intentionally left empty. This is a production repair migration that
            // normalizes table names to match the current EF model. Reverting table
            // names would reintroduce the runtime "Invalid object name" failures.
        }

        private static void RenameTablesToUppercase(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DECLARE @TableRenames TABLE
(
    OldName sysname NOT NULL,
    NewName sysname NOT NULL
);

INSERT INTO @TableRenames (OldName, NewName)
VALUES
    (N'RII_WelfareAssessment', N'RII_WELFARE_ASSESSMENT'),
    (N'RII_WeighingLine', N'RII_WEIGHING_LINE'),
    (N'RII_WeatherType', N'RII_WEATHER_TYPE'),
    (N'RII_WeatherSeverity', N'RII_WEATHER_SEVERITY'),
    (N'RII_WarehouseTransferLine', N'RII_WAREHOUSE_TRANSFER_LINE'),
    (N'RII_WarehouseTransfer', N'RII_WAREHOUSE_TRANSFER'),
    (N'RII_WarehouseCageTransferLine', N'RII_WAREHOUSE_CAGE_TRANSFER_LINE'),
    (N'RII_WarehouseCageTransfer', N'RII_WAREHOUSE_CAGE_TRANSFER'),
    (N'RII_TransferLine', N'RII_TRANSFER_LINE'),
    (N'RII_StockConvertLine', N'RII_STOCK_CONVERT_LINE'),
    (N'RII_StockConvert', N'RII_STOCK_CONVERT'),
    (N'RII_ShipmentLine', N'RII_SHIPMENT_LINE'),
    (N'RII_ProjectMergeSource', N'RII_PROJECT_MERGE_SOURCE'),
    (N'RII_ProjectMergeCage', N'RII_PROJECT_MERGE_CAGE'),
    (N'RII_ProjectMerge', N'RII_PROJECT_MERGE'),
    (N'RII_ProjectCageDailyKpiSnapshot', N'RII_PROJECT_CAGE_DAILY_KPI_SNAPSHOT'),
    (N'RII_ProjectCage', N'RII_PROJECT_CAGE'),
    (N'RII_OpeningImportRow', N'RII_OPENING_IMPORT_ROW'),
    (N'RII_OpeningImportJob', N'RII_OPENING_IMPORT_JOB'),
    (N'RII_NetOperationType', N'RII_NET_OPERATION_TYPE'),
    (N'RII_NetOperationLine', N'RII_NET_OPERATION_LINE'),
    (N'RII_NetOperation', N'RII_NET_OPERATION'),
    (N'RII_NetInventoryMovement', N'RII_NET_INVENTORY_MOVEMENT'),
    (N'RII_MortalityLine', N'RII_MORTALITY_LINE'),
    (N'RII_GoodsReceiptLine', N'RII_GOODS_RECEIPT_LINE'),
    (N'RII_GoodsReceiptFishDistribution', N'RII_GOODS_RECEIPT_FISH_DISTRIBUTION'),
    (N'RII_GoodsReceipt', N'RII_GOODS_RECEIPT'),
    (N'RII_FishTreatment', N'RII_FISH_TREATMENT'),
    (N'RII_FishLabSample', N'RII_FISH_LAB_SAMPLE'),
    (N'RII_FishLabResult', N'RII_FISH_LAB_RESULT'),
    (N'RII_FishHealthEvent', N'RII_FISH_HEALTH_EVENT'),
    (N'RII_FishBatch', N'RII_FISH_BATCH'),
    (N'RII_FeedingLine', N'RII_FEEDING_LINE'),
    (N'RII_FeedingDistribution', N'RII_FEEDING_DISTRIBUTION'),
    (N'RII_DailyWeather', N'RII_DAILY_WEATHER'),
    (N'RII_ComplianceCorrectiveAction', N'RII_COMPLIANCE_CORRECTIVE_ACTION'),
    (N'RII_ComplianceAudit', N'RII_COMPLIANCE_AUDIT'),
    (N'RII_CageWarehouseTransferLine', N'RII_CAGE_WAREHOUSE_TRANSFER_LINE'),
    (N'RII_CageWarehouseTransfer', N'RII_CAGE_WAREHOUSE_TRANSFER'),
    (N'RII_CageWarehouseMapping', N'RII_CAGE_WAREHOUSE_MAPPING'),
    (N'RII_BatchWarehouseBalance', N'RII_BATCH_WAREHOUSE_BALANCE'),
    (N'RII_BatchMovement', N'RII_BATCH_MOVEMENT'),
    (N'RII_BatchCageBalance', N'RII_BATCH_CAGE_BALANCE'),
    (N'RII_AquaSetting', N'RII_AQUA_SETTING');

DECLARE @OldName sysname;
DECLARE @NewName sysname;
DECLARE @Sql nvarchar(max);

DECLARE table_cursor CURSOR LOCAL FAST_FORWARD FOR
    SELECT OldName, NewName
    FROM @TableRenames
    ORDER BY OldName;

OPEN table_cursor;
FETCH NEXT FROM table_cursor INTO @OldName, @NewName;

WHILE @@FETCH_STATUS = 0
BEGIN
    IF OBJECT_ID(QUOTENAME(N'dbo') + N'.' + QUOTENAME(@OldName), N'U') IS NOT NULL
       AND OBJECT_ID(QUOTENAME(N'dbo') + N'.' + QUOTENAME(@NewName), N'U') IS NULL
    BEGIN
        SET @Sql = N'EXEC sp_rename N''[dbo].[' + REPLACE(@OldName, N'''', N'''''') + N']'', N''' + REPLACE(@NewName, N'''', N'''''') + N'''';
        EXEC sp_executesql @Sql;
    END

    FETCH NEXT FROM table_cursor INTO @OldName, @NewName;
END

CLOSE table_cursor;
DEALLOCATE table_cursor;
");
        }
    }
}
