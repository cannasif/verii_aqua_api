using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace aqua_api.Migrations
{
    /// <inheritdoc />
    public partial class StandardizeDecimalPrecisionTo28_8 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "WelfareScore",
                table: "RII_WELFARE_ASSESSMENT",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)",
                oldPrecision: 18,
                oldScale: 3);

            migrationBuilder.AlterColumn<decimal>(
                name: "StockingDensityKgM3",
                table: "RII_WELFARE_ASSESSMENT",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)",
                oldPrecision: 18,
                oldScale: 3,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "SkinScore",
                table: "RII_WELFARE_ASSESSMENT",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)",
                oldPrecision: 18,
                oldScale: 3,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "GillScore",
                table: "RII_WELFARE_ASSESSMENT",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)",
                oldPrecision: 18,
                oldScale: 3,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "FinScore",
                table: "RII_WELFARE_ASSESSMENT",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)",
                oldPrecision: 18,
                oldScale: 3,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "BehaviorScore",
                table: "RII_WELFARE_ASSESSMENT",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)",
                oldPrecision: 18,
                oldScale: 3,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "AppetiteScore",
                table: "RII_WELFARE_ASSESSMENT",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)",
                oldPrecision: 18,
                oldScale: 3,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "MeasuredBiomassGram",
                table: "RII_WEIGHING_LINE",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)",
                oldPrecision: 18,
                oldScale: 3);

            migrationBuilder.AlterColumn<decimal>(
                name: "MeasuredAverageGram",
                table: "RII_WEIGHING_LINE",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)",
                oldPrecision: 18,
                oldScale: 3);

            migrationBuilder.AlterColumn<decimal>(
                name: "BiomassGram",
                table: "RII_WAREHOUSE_TRANSFER_LINE",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)",
                oldPrecision: 18,
                oldScale: 3);

            migrationBuilder.AlterColumn<decimal>(
                name: "AverageGram",
                table: "RII_WAREHOUSE_TRANSFER_LINE",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)",
                oldPrecision: 18,
                oldScale: 3);

            migrationBuilder.AlterColumn<decimal>(
                name: "BiomassGram",
                table: "RII_WAREHOUSE_CAGE_TRANSFER_LINE",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)",
                oldPrecision: 18,
                oldScale: 3);

            migrationBuilder.AlterColumn<decimal>(
                name: "AverageGram",
                table: "RII_WAREHOUSE_CAGE_TRANSFER_LINE",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)",
                oldPrecision: 18,
                oldScale: 3);

            migrationBuilder.AlterColumn<decimal>(
                name: "Weight",
                table: "RII_USER_DETAIL",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(5,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Height",
                table: "RII_USER_DETAIL",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(5,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "BiomassGram",
                table: "RII_TRANSFER_LINE",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)",
                oldPrecision: 18,
                oldScale: 3);

            migrationBuilder.AlterColumn<decimal>(
                name: "AverageGram",
                table: "RII_TRANSFER_LINE",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)",
                oldPrecision: 18,
                oldScale: 3);

            migrationBuilder.AlterColumn<decimal>(
                name: "Quantity",
                table: "RII_STOCK_RELATION",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)");

            migrationBuilder.AlterColumn<decimal>(
                name: "NewAverageGram",
                table: "RII_STOCK_CONVERT_LINE",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)",
                oldPrecision: 18,
                oldScale: 3);

            migrationBuilder.AlterColumn<decimal>(
                name: "BiomassGram",
                table: "RII_STOCK_CONVERT_LINE",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)",
                oldPrecision: 18,
                oldScale: 3);

            migrationBuilder.AlterColumn<decimal>(
                name: "AverageGram",
                table: "RII_STOCK_CONVERT_LINE",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)",
                oldPrecision: 18,
                oldScale: 3);

            migrationBuilder.AlterColumn<decimal>(
                name: "UnitPrice",
                table: "RII_SHIPMENT_LINE",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)",
                oldPrecision: 18,
                oldScale: 6,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "LocalUnitPrice",
                table: "RII_SHIPMENT_LINE",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)",
                oldPrecision: 18,
                oldScale: 6,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "LocalLineAmount",
                table: "RII_SHIPMENT_LINE",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)",
                oldPrecision: 18,
                oldScale: 6,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "LineAmount",
                table: "RII_SHIPMENT_LINE",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)",
                oldPrecision: 18,
                oldScale: 6,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "ExchangeRate",
                table: "RII_SHIPMENT_LINE",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)",
                oldPrecision: 18,
                oldScale: 6,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "BiomassGram",
                table: "RII_SHIPMENT_LINE",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)",
                oldPrecision: 18,
                oldScale: 3);

            migrationBuilder.AlterColumn<decimal>(
                name: "AverageGram",
                table: "RII_SHIPMENT_LINE",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)",
                oldPrecision: 18,
                oldScale: 3);

            migrationBuilder.AlterColumn<decimal>(
                name: "WaterTemperatureCelsius",
                table: "RII_SEA_WATER_TEMPERATURE",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,3)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "SurvivalPct",
                table: "RII_PROJECT_CAGE_DAILY_KPI_SNAPSHOT",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)",
                oldPrecision: 9,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "Sgr",
                table: "RII_PROJECT_CAGE_DAILY_KPI_SNAPSHOT",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)",
                oldPrecision: 18,
                oldScale: 4);

            migrationBuilder.AlterColumn<decimal>(
                name: "MortalityPctPeriod",
                table: "RII_PROJECT_CAGE_DAILY_KPI_SNAPSHOT",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)",
                oldPrecision: 9,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "HarvestReadinessScore",
                table: "RII_PROJECT_CAGE_DAILY_KPI_SNAPSHOT",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)",
                oldPrecision: 9,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "ForecastBiomassKg30Days",
                table: "RII_PROJECT_CAGE_DAILY_KPI_SNAPSHOT",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)",
                oldPrecision: 18,
                oldScale: 3);

            migrationBuilder.AlterColumn<decimal>(
                name: "FeedKgPeriod",
                table: "RII_PROJECT_CAGE_DAILY_KPI_SNAPSHOT",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)",
                oldPrecision: 18,
                oldScale: 3);

            migrationBuilder.AlterColumn<decimal>(
                name: "Fcr",
                table: "RII_PROJECT_CAGE_DAILY_KPI_SNAPSHOT",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)",
                oldPrecision: 18,
                oldScale: 4);

            migrationBuilder.AlterColumn<decimal>(
                name: "DataQualityScore",
                table: "RII_PROJECT_CAGE_DAILY_KPI_SNAPSHOT",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)",
                oldPrecision: 9,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "CapacityUsagePct",
                table: "RII_PROJECT_CAGE_DAILY_KPI_SNAPSHOT",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)",
                oldPrecision: 9,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "BiomassKg",
                table: "RII_PROJECT_CAGE_DAILY_KPI_SNAPSHOT",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)",
                oldPrecision: 18,
                oldScale: 3);

            migrationBuilder.AlterColumn<decimal>(
                name: "BiomassGainKgPeriod",
                table: "RII_PROJECT_CAGE_DAILY_KPI_SNAPSHOT",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)",
                oldPrecision: 18,
                oldScale: 3);

            migrationBuilder.AlterColumn<decimal>(
                name: "AverageGram",
                table: "RII_PROJECT_CAGE_DAILY_KPI_SNAPSHOT",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)",
                oldPrecision: 18,
                oldScale: 3);

            migrationBuilder.AlterColumn<decimal>(
                name: "Adg",
                table: "RII_PROJECT_CAGE_DAILY_KPI_SNAPSHOT",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)",
                oldPrecision: 18,
                oldScale: 4);

            migrationBuilder.AlterColumn<decimal>(
                name: "UnitPrice",
                table: "RII_GOODS_RECEIPT_LINE",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)",
                oldPrecision: 18,
                oldScale: 6,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "TotalGram",
                table: "RII_GOODS_RECEIPT_LINE",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)",
                oldPrecision: 18,
                oldScale: 3,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "QtyUnit",
                table: "RII_GOODS_RECEIPT_LINE",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)",
                oldPrecision: 18,
                oldScale: 3,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "LocalUnitPrice",
                table: "RII_GOODS_RECEIPT_LINE",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)",
                oldPrecision: 18,
                oldScale: 6,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "LocalLineAmount",
                table: "RII_GOODS_RECEIPT_LINE",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)",
                oldPrecision: 18,
                oldScale: 6,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "LineAmount",
                table: "RII_GOODS_RECEIPT_LINE",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)",
                oldPrecision: 18,
                oldScale: 6,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "GramPerUnit",
                table: "RII_GOODS_RECEIPT_LINE",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)",
                oldPrecision: 18,
                oldScale: 3,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "FishTotalGram",
                table: "RII_GOODS_RECEIPT_LINE",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)",
                oldPrecision: 18,
                oldScale: 3,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "FishAverageGram",
                table: "RII_GOODS_RECEIPT_LINE",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)",
                oldPrecision: 18,
                oldScale: 3,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "ExchangeRate",
                table: "RII_GOODS_RECEIPT_LINE",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)",
                oldPrecision: 18,
                oldScale: 6,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "DoseValue",
                table: "RII_FISH_TREATMENT",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)",
                oldPrecision: 18,
                oldScale: 3,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "AffectedRatioPct",
                table: "RII_FISH_HEALTH_EVENT",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)",
                oldPrecision: 18,
                oldScale: 3,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "PreviousBiomassGram",
                table: "RII_FISH_GROWTH",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)",
                oldPrecision: 18,
                oldScale: 3);

            migrationBuilder.AlterColumn<decimal>(
                name: "PreviousAverageGram",
                table: "RII_FISH_GROWTH",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)",
                oldPrecision: 18,
                oldScale: 3);

            migrationBuilder.AlterColumn<decimal>(
                name: "NewBiomassGram",
                table: "RII_FISH_GROWTH",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)",
                oldPrecision: 18,
                oldScale: 3);

            migrationBuilder.AlterColumn<decimal>(
                name: "NewAverageGram",
                table: "RII_FISH_GROWTH",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)",
                oldPrecision: 18,
                oldScale: 3);

            migrationBuilder.AlterColumn<decimal>(
                name: "GrowthGram",
                table: "RII_FISH_GROWTH",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)",
                oldPrecision: 18,
                oldScale: 3);

            migrationBuilder.AlterColumn<decimal>(
                name: "TargetHarvestAverageGram",
                table: "RII_FISH_BATCH",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)",
                oldPrecision: 18,
                oldScale: 3,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "CurrentAverageGram",
                table: "RII_FISH_BATCH",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)",
                oldPrecision: 18,
                oldScale: 3);

            migrationBuilder.AlterColumn<decimal>(
                name: "TotalGram",
                table: "RII_FEEDING_LINE",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)",
                oldPrecision: 18,
                oldScale: 3);

            migrationBuilder.AlterColumn<decimal>(
                name: "QtyUnit",
                table: "RII_FEEDING_LINE",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)",
                oldPrecision: 18,
                oldScale: 3);

            migrationBuilder.AlterColumn<decimal>(
                name: "GramPerUnit",
                table: "RII_FEEDING_LINE",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)",
                oldPrecision: 18,
                oldScale: 3);

            migrationBuilder.AlterColumn<decimal>(
                name: "FeedGram",
                table: "RII_FEEDING_DISTRIBUTION",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)",
                oldPrecision: 18,
                oldScale: 3);

            migrationBuilder.AlterColumn<decimal>(
                name: "WindKnot",
                table: "RII_DAILY_WEATHER",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)",
                oldPrecision: 18,
                oldScale: 3,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "WaveHeightM",
                table: "RII_DAILY_WEATHER",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)",
                oldPrecision: 18,
                oldScale: 3,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "WaterTemperatureSurfaceC",
                table: "RII_DAILY_WEATHER",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)",
                oldPrecision: 18,
                oldScale: 3,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "WaterTemperatureDepthC",
                table: "RII_DAILY_WEATHER",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)",
                oldPrecision: 18,
                oldScale: 3,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "TurbidityNtu",
                table: "RII_DAILY_WEATHER",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)",
                oldPrecision: 18,
                oldScale: 3,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "TemperatureC",
                table: "RII_DAILY_WEATHER",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)",
                oldPrecision: 18,
                oldScale: 3,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "SensorHealthScore",
                table: "RII_DAILY_WEATHER",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)",
                oldPrecision: 18,
                oldScale: 3,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "SalinityPpt",
                table: "RII_DAILY_WEATHER",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)",
                oldPrecision: 18,
                oldScale: 3,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Ph",
                table: "RII_DAILY_WEATHER",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)",
                oldPrecision: 18,
                oldScale: 3,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "NitriteMgL",
                table: "RII_DAILY_WEATHER",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)",
                oldPrecision: 18,
                oldScale: 3,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "DissolvedOxygenMgL",
                table: "RII_DAILY_WEATHER",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)",
                oldPrecision: 18,
                oldScale: 3,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "CurrentSpeedKn",
                table: "RII_DAILY_WEATHER",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)",
                oldPrecision: 18,
                oldScale: 3,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "AmmoniaMgL",
                table: "RII_DAILY_WEATHER",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)",
                oldPrecision: 18,
                oldScale: 3,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "AlgalBloomIndex",
                table: "RII_DAILY_WEATHER",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)",
                oldPrecision: 18,
                oldScale: 3,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "BiomassGram",
                table: "RII_CAGE_WAREHOUSE_TRANSFER_LINE",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)",
                oldPrecision: 18,
                oldScale: 3);

            migrationBuilder.AlterColumn<decimal>(
                name: "AverageGram",
                table: "RII_CAGE_WAREHOUSE_TRANSFER_LINE",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)",
                oldPrecision: 18,
                oldScale: 3);

            migrationBuilder.AlterColumn<decimal>(
                name: "CapacityGram",
                table: "RII_CAGE",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)",
                oldPrecision: 18,
                oldScale: 3,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "WaterTemperatureCelsius",
                table: "RII_BUDGET_WATER_TEMPERATURE",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,3)");

            migrationBuilder.AlterColumn<decimal>(
                name: "UnitPrice",
                table: "RII_BUDGET_PLAN_SALES_LINE",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "SourceUnitPrice",
                table: "RII_BUDGET_PLAN_SALES_LINE",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "SalesTon",
                table: "RII_BUDGET_PLAN_SALES_LINE",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)");

            migrationBuilder.AlterColumn<decimal>(
                name: "UnitPriceEuro",
                table: "RII_BUDGET_PLAN_SALES_DISTRIBUTION",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)");

            migrationBuilder.AlterColumn<decimal>(
                name: "UnitPrice",
                table: "RII_BUDGET_PLAN_SALES_DISTRIBUTION",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)");

            migrationBuilder.AlterColumn<decimal>(
                name: "UnitGram",
                table: "RII_BUDGET_PLAN_SALES_DISTRIBUTION",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)");

            migrationBuilder.AlterColumn<decimal>(
                name: "SalesTon",
                table: "RII_BUDGET_PLAN_SALES_DISTRIBUTION",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)");

            migrationBuilder.AlterColumn<decimal>(
                name: "SalesKg",
                table: "RII_BUDGET_PLAN_SALES_DISTRIBUTION",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)");

            migrationBuilder.AlterColumn<decimal>(
                name: "ExchangeRate",
                table: "RII_BUDGET_PLAN_SALES_DISTRIBUTION",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "AmountTry",
                table: "RII_BUDGET_PLAN_SALES_DISTRIBUTION",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "AmountEuro",
                table: "RII_BUDGET_PLAN_SALES_DISTRIBUTION",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)");

            migrationBuilder.AlterColumn<decimal>(
                name: "Amount",
                table: "RII_BUDGET_PLAN_SALES_DISTRIBUTION",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)");

            migrationBuilder.AlterColumn<decimal>(
                name: "MortalityRatePercent",
                table: "RII_BUDGET_PLAN_MORTALITY_LINE",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)");

            migrationBuilder.AlterColumn<decimal>(
                name: "MortalityKg",
                table: "RII_BUDGET_PLAN_MORTALITY_LINE",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)");

            migrationBuilder.AlterColumn<decimal>(
                name: "SalesTon",
                table: "RII_BUDGET_PLAN_MONTHLY_PROJECTION",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)");

            migrationBuilder.AlterColumn<decimal>(
                name: "RawMonthlyGrowthGram",
                table: "RII_BUDGET_PLAN_MONTHLY_PROJECTION",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)");

            migrationBuilder.AlterColumn<decimal>(
                name: "OpeningBiomassKg",
                table: "RII_BUDGET_PLAN_MONTHLY_PROJECTION",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)");

            migrationBuilder.AlterColumn<decimal>(
                name: "OpeningAverageGram",
                table: "RII_BUDGET_PLAN_MONTHLY_PROJECTION",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)");

            migrationBuilder.AlterColumn<decimal>(
                name: "MortalityKg",
                table: "RII_BUDGET_PLAN_MONTHLY_PROJECTION",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)");

            migrationBuilder.AlterColumn<decimal>(
                name: "MonthlyGrowthGram",
                table: "RII_BUDGET_PLAN_MONTHLY_PROJECTION",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)");

            migrationBuilder.AlterColumn<decimal>(
                name: "GrowthQualityPercent",
                table: "RII_BUDGET_PLAN_MONTHLY_PROJECTION",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: false,
                defaultValue: 100m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)",
                oldDefaultValue: 100m);

            migrationBuilder.AlterColumn<decimal>(
                name: "FeedMortalityReductionPercent",
                table: "RII_BUDGET_PLAN_MONTHLY_PROJECTION",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)");

            migrationBuilder.AlterColumn<decimal>(
                name: "FeedMortalityReductionKg",
                table: "RII_BUDGET_PLAN_MONTHLY_PROJECTION",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)");

            migrationBuilder.AlterColumn<decimal>(
                name: "FeedKg",
                table: "RII_BUDGET_PLAN_MONTHLY_PROJECTION",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)");

            migrationBuilder.AlterColumn<decimal>(
                name: "ClosingBiomassKg",
                table: "RII_BUDGET_PLAN_MONTHLY_PROJECTION",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)");

            migrationBuilder.AlterColumn<decimal>(
                name: "ClosingAverageGram",
                table: "RII_BUDGET_PLAN_MONTHLY_PROJECTION",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)");

            migrationBuilder.AlterColumn<decimal>(
                name: "UnitPrice",
                table: "RII_BUDGET_PLAN_FISH_PRICE",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)",
                oldPrecision: 18,
                oldScale: 6);

            migrationBuilder.AlterColumn<decimal>(
                name: "IncreaseRatePercent",
                table: "RII_BUDGET_PLAN_FISH_PRICE",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)",
                oldPrecision: 18,
                oldScale: 6);

            migrationBuilder.AlterColumn<decimal>(
                name: "BiomassKg",
                table: "RII_BUDGET_PLAN_FISH_BATCH_ADJUSTMENT",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)");

            migrationBuilder.AlterColumn<decimal>(
                name: "AverageGram",
                table: "RII_BUDGET_PLAN_FISH_BATCH_ADJUSTMENT",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)");

            migrationBuilder.AlterColumn<decimal>(
                name: "InitialUnitCost",
                table: "RII_BUDGET_PLAN_FISH_BATCH",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)",
                oldPrecision: 18,
                oldScale: 6,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "InitialSmmAmount",
                table: "RII_BUDGET_PLAN_FISH_BATCH",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)",
                oldPrecision: 18,
                oldScale: 6,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "InitialBiomassKg",
                table: "RII_BUDGET_PLAN_FISH_BATCH",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)");

            migrationBuilder.AlterColumn<decimal>(
                name: "InitialAverageGram",
                table: "RII_BUDGET_PLAN_FISH_BATCH",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)");

            migrationBuilder.AlterColumn<decimal>(
                name: "MortalityReductionPercent",
                table: "RII_BUDGET_PLAN_FEEDING_LINE",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)");

            migrationBuilder.AlterColumn<decimal>(
                name: "MortalityReductionKg",
                table: "RII_BUDGET_PLAN_FEEDING_LINE",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)");

            migrationBuilder.AlterColumn<decimal>(
                name: "FeedKg",
                table: "RII_BUDGET_PLAN_FEEDING_LINE",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)");

            migrationBuilder.AlterColumn<decimal>(
                name: "FeedAmountRate",
                table: "RII_BUDGET_PLAN_FEEDING_LINE",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)");

            migrationBuilder.AlterColumn<decimal>(
                name: "ExchangeRate",
                table: "RII_BUDGET_PLAN_EXCHANGE_RATE",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)",
                oldPrecision: 18,
                oldScale: 6);

            migrationBuilder.AlterColumn<decimal>(
                name: "MortalityRatePercent",
                table: "RII_BUDGET_MORTALITY_RATE_DEFINITION",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)");

            migrationBuilder.AlterColumn<decimal>(
                name: "QualityPercent",
                table: "RII_BUDGET_FISH_GROWTH_QUALITY",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)");

            migrationBuilder.AlterColumn<decimal>(
                name: "TotalGram",
                table: "RII_BUDGET_FISH_GROWTH_PROFILE_LINE",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,8)");

            migrationBuilder.AlterColumn<decimal>(
                name: "MonthlyGrowthGram",
                table: "RII_BUDGET_FISH_GROWTH_PROFILE_LINE",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,8)");

            migrationBuilder.AlterColumn<decimal>(
                name: "ReductionRatePercent",
                table: "RII_BUDGET_FEED_MORTALITY_RATE",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)");

            migrationBuilder.AlterColumn<decimal>(
                name: "FeedAmount",
                table: "RII_BUDGET_FEED_CONSUMPTION_RATE",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)");

            migrationBuilder.AlterColumn<decimal>(
                name: "BiomassGram",
                table: "RII_BATCH_WAREHOUSE_BALANCE",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)",
                oldPrecision: 18,
                oldScale: 3);

            migrationBuilder.AlterColumn<decimal>(
                name: "AverageGram",
                table: "RII_BATCH_WAREHOUSE_BALANCE",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)",
                oldPrecision: 18,
                oldScale: 3);

            migrationBuilder.AlterColumn<decimal>(
                name: "ToAverageGram",
                table: "RII_BATCH_MOVEMENT",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)",
                oldPrecision: 18,
                oldScale: 3,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "SignedBiomassGram",
                table: "RII_BATCH_MOVEMENT",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)",
                oldPrecision: 18,
                oldScale: 3);

            migrationBuilder.AlterColumn<decimal>(
                name: "ReportedBiomassGram",
                table: "RII_BATCH_MOVEMENT",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)",
                oldPrecision: 18,
                oldScale: 3,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "FromAverageGram",
                table: "RII_BATCH_MOVEMENT",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)",
                oldPrecision: 18,
                oldScale: 3,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "FeedGram",
                table: "RII_BATCH_MOVEMENT",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)",
                oldPrecision: 18,
                oldScale: 3,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "BiomassGram",
                table: "RII_BATCH_CAGE_BALANCE",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)",
                oldPrecision: 18,
                oldScale: 3);

            migrationBuilder.AlterColumn<decimal>(
                name: "AverageGram",
                table: "RII_BATCH_CAGE_BALANCE",
                type: "decimal(28,8)",
                precision: 28,
                scale: 8,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)",
                oldPrecision: 18,
                oldScale: 3);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "WelfareScore",
                table: "RII_WELFARE_ASSESSMENT",
                type: "decimal(18,6)",
                precision: 18,
                scale: 3,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8);

            migrationBuilder.AlterColumn<decimal>(
                name: "StockingDensityKgM3",
                table: "RII_WELFARE_ASSESSMENT",
                type: "decimal(18,6)",
                precision: 18,
                scale: 3,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "SkinScore",
                table: "RII_WELFARE_ASSESSMENT",
                type: "decimal(18,6)",
                precision: 18,
                scale: 3,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "GillScore",
                table: "RII_WELFARE_ASSESSMENT",
                type: "decimal(18,6)",
                precision: 18,
                scale: 3,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "FinScore",
                table: "RII_WELFARE_ASSESSMENT",
                type: "decimal(18,6)",
                precision: 18,
                scale: 3,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "BehaviorScore",
                table: "RII_WELFARE_ASSESSMENT",
                type: "decimal(18,6)",
                precision: 18,
                scale: 3,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "AppetiteScore",
                table: "RII_WELFARE_ASSESSMENT",
                type: "decimal(18,6)",
                precision: 18,
                scale: 3,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "MeasuredBiomassGram",
                table: "RII_WEIGHING_LINE",
                type: "decimal(18,6)",
                precision: 18,
                scale: 3,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8);

            migrationBuilder.AlterColumn<decimal>(
                name: "MeasuredAverageGram",
                table: "RII_WEIGHING_LINE",
                type: "decimal(18,6)",
                precision: 18,
                scale: 3,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8);

            migrationBuilder.AlterColumn<decimal>(
                name: "BiomassGram",
                table: "RII_WAREHOUSE_TRANSFER_LINE",
                type: "decimal(18,6)",
                precision: 18,
                scale: 3,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8);

            migrationBuilder.AlterColumn<decimal>(
                name: "AverageGram",
                table: "RII_WAREHOUSE_TRANSFER_LINE",
                type: "decimal(18,6)",
                precision: 18,
                scale: 3,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8);

            migrationBuilder.AlterColumn<decimal>(
                name: "BiomassGram",
                table: "RII_WAREHOUSE_CAGE_TRANSFER_LINE",
                type: "decimal(18,6)",
                precision: 18,
                scale: 3,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8);

            migrationBuilder.AlterColumn<decimal>(
                name: "AverageGram",
                table: "RII_WAREHOUSE_CAGE_TRANSFER_LINE",
                type: "decimal(18,6)",
                precision: 18,
                scale: 3,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8);

            migrationBuilder.AlterColumn<decimal>(
                name: "Weight",
                table: "RII_USER_DETAIL",
                type: "decimal(5,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Height",
                table: "RII_USER_DETAIL",
                type: "decimal(5,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "BiomassGram",
                table: "RII_TRANSFER_LINE",
                type: "decimal(18,6)",
                precision: 18,
                scale: 3,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8);

            migrationBuilder.AlterColumn<decimal>(
                name: "AverageGram",
                table: "RII_TRANSFER_LINE",
                type: "decimal(18,6)",
                precision: 18,
                scale: 3,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8);

            migrationBuilder.AlterColumn<decimal>(
                name: "Quantity",
                table: "RII_STOCK_RELATION",
                type: "decimal(18,6)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8);

            migrationBuilder.AlterColumn<decimal>(
                name: "NewAverageGram",
                table: "RII_STOCK_CONVERT_LINE",
                type: "decimal(18,6)",
                precision: 18,
                scale: 3,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8);

            migrationBuilder.AlterColumn<decimal>(
                name: "BiomassGram",
                table: "RII_STOCK_CONVERT_LINE",
                type: "decimal(18,6)",
                precision: 18,
                scale: 3,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8);

            migrationBuilder.AlterColumn<decimal>(
                name: "AverageGram",
                table: "RII_STOCK_CONVERT_LINE",
                type: "decimal(18,6)",
                precision: 18,
                scale: 3,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8);

            migrationBuilder.AlterColumn<decimal>(
                name: "UnitPrice",
                table: "RII_SHIPMENT_LINE",
                type: "decimal(18,6)",
                precision: 18,
                scale: 6,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "LocalUnitPrice",
                table: "RII_SHIPMENT_LINE",
                type: "decimal(18,6)",
                precision: 18,
                scale: 6,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "LocalLineAmount",
                table: "RII_SHIPMENT_LINE",
                type: "decimal(18,6)",
                precision: 18,
                scale: 6,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "LineAmount",
                table: "RII_SHIPMENT_LINE",
                type: "decimal(18,6)",
                precision: 18,
                scale: 6,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "ExchangeRate",
                table: "RII_SHIPMENT_LINE",
                type: "decimal(18,6)",
                precision: 18,
                scale: 6,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "BiomassGram",
                table: "RII_SHIPMENT_LINE",
                type: "decimal(18,6)",
                precision: 18,
                scale: 3,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8);

            migrationBuilder.AlterColumn<decimal>(
                name: "AverageGram",
                table: "RII_SHIPMENT_LINE",
                type: "decimal(18,6)",
                precision: 18,
                scale: 3,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8);

            migrationBuilder.AlterColumn<decimal>(
                name: "WaterTemperatureCelsius",
                table: "RII_SEA_WATER_TEMPERATURE",
                type: "decimal(18,3)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "SurvivalPct",
                table: "RII_PROJECT_CAGE_DAILY_KPI_SNAPSHOT",
                type: "decimal(18,6)",
                precision: 9,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8);

            migrationBuilder.AlterColumn<decimal>(
                name: "Sgr",
                table: "RII_PROJECT_CAGE_DAILY_KPI_SNAPSHOT",
                type: "decimal(18,6)",
                precision: 18,
                scale: 4,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8);

            migrationBuilder.AlterColumn<decimal>(
                name: "MortalityPctPeriod",
                table: "RII_PROJECT_CAGE_DAILY_KPI_SNAPSHOT",
                type: "decimal(18,6)",
                precision: 9,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8);

            migrationBuilder.AlterColumn<decimal>(
                name: "HarvestReadinessScore",
                table: "RII_PROJECT_CAGE_DAILY_KPI_SNAPSHOT",
                type: "decimal(18,6)",
                precision: 9,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8);

            migrationBuilder.AlterColumn<decimal>(
                name: "ForecastBiomassKg30Days",
                table: "RII_PROJECT_CAGE_DAILY_KPI_SNAPSHOT",
                type: "decimal(18,6)",
                precision: 18,
                scale: 3,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8);

            migrationBuilder.AlterColumn<decimal>(
                name: "FeedKgPeriod",
                table: "RII_PROJECT_CAGE_DAILY_KPI_SNAPSHOT",
                type: "decimal(18,6)",
                precision: 18,
                scale: 3,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8);

            migrationBuilder.AlterColumn<decimal>(
                name: "Fcr",
                table: "RII_PROJECT_CAGE_DAILY_KPI_SNAPSHOT",
                type: "decimal(18,6)",
                precision: 18,
                scale: 4,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8);

            migrationBuilder.AlterColumn<decimal>(
                name: "DataQualityScore",
                table: "RII_PROJECT_CAGE_DAILY_KPI_SNAPSHOT",
                type: "decimal(18,6)",
                precision: 9,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8);

            migrationBuilder.AlterColumn<decimal>(
                name: "CapacityUsagePct",
                table: "RII_PROJECT_CAGE_DAILY_KPI_SNAPSHOT",
                type: "decimal(18,6)",
                precision: 9,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8);

            migrationBuilder.AlterColumn<decimal>(
                name: "BiomassKg",
                table: "RII_PROJECT_CAGE_DAILY_KPI_SNAPSHOT",
                type: "decimal(18,6)",
                precision: 18,
                scale: 3,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8);

            migrationBuilder.AlterColumn<decimal>(
                name: "BiomassGainKgPeriod",
                table: "RII_PROJECT_CAGE_DAILY_KPI_SNAPSHOT",
                type: "decimal(18,6)",
                precision: 18,
                scale: 3,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8);

            migrationBuilder.AlterColumn<decimal>(
                name: "AverageGram",
                table: "RII_PROJECT_CAGE_DAILY_KPI_SNAPSHOT",
                type: "decimal(18,6)",
                precision: 18,
                scale: 3,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8);

            migrationBuilder.AlterColumn<decimal>(
                name: "Adg",
                table: "RII_PROJECT_CAGE_DAILY_KPI_SNAPSHOT",
                type: "decimal(18,6)",
                precision: 18,
                scale: 4,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8);

            migrationBuilder.AlterColumn<decimal>(
                name: "UnitPrice",
                table: "RII_GOODS_RECEIPT_LINE",
                type: "decimal(18,6)",
                precision: 18,
                scale: 6,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "TotalGram",
                table: "RII_GOODS_RECEIPT_LINE",
                type: "decimal(18,6)",
                precision: 18,
                scale: 3,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "QtyUnit",
                table: "RII_GOODS_RECEIPT_LINE",
                type: "decimal(18,6)",
                precision: 18,
                scale: 3,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "LocalUnitPrice",
                table: "RII_GOODS_RECEIPT_LINE",
                type: "decimal(18,6)",
                precision: 18,
                scale: 6,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "LocalLineAmount",
                table: "RII_GOODS_RECEIPT_LINE",
                type: "decimal(18,6)",
                precision: 18,
                scale: 6,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "LineAmount",
                table: "RII_GOODS_RECEIPT_LINE",
                type: "decimal(18,6)",
                precision: 18,
                scale: 6,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "GramPerUnit",
                table: "RII_GOODS_RECEIPT_LINE",
                type: "decimal(18,6)",
                precision: 18,
                scale: 3,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "FishTotalGram",
                table: "RII_GOODS_RECEIPT_LINE",
                type: "decimal(18,6)",
                precision: 18,
                scale: 3,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "FishAverageGram",
                table: "RII_GOODS_RECEIPT_LINE",
                type: "decimal(18,6)",
                precision: 18,
                scale: 3,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "ExchangeRate",
                table: "RII_GOODS_RECEIPT_LINE",
                type: "decimal(18,6)",
                precision: 18,
                scale: 6,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "DoseValue",
                table: "RII_FISH_TREATMENT",
                type: "decimal(18,6)",
                precision: 18,
                scale: 3,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "AffectedRatioPct",
                table: "RII_FISH_HEALTH_EVENT",
                type: "decimal(18,6)",
                precision: 18,
                scale: 3,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "PreviousBiomassGram",
                table: "RII_FISH_GROWTH",
                type: "decimal(18,6)",
                precision: 18,
                scale: 3,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8);

            migrationBuilder.AlterColumn<decimal>(
                name: "PreviousAverageGram",
                table: "RII_FISH_GROWTH",
                type: "decimal(18,6)",
                precision: 18,
                scale: 3,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8);

            migrationBuilder.AlterColumn<decimal>(
                name: "NewBiomassGram",
                table: "RII_FISH_GROWTH",
                type: "decimal(18,6)",
                precision: 18,
                scale: 3,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8);

            migrationBuilder.AlterColumn<decimal>(
                name: "NewAverageGram",
                table: "RII_FISH_GROWTH",
                type: "decimal(18,6)",
                precision: 18,
                scale: 3,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8);

            migrationBuilder.AlterColumn<decimal>(
                name: "GrowthGram",
                table: "RII_FISH_GROWTH",
                type: "decimal(18,6)",
                precision: 18,
                scale: 3,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8);

            migrationBuilder.AlterColumn<decimal>(
                name: "TargetHarvestAverageGram",
                table: "RII_FISH_BATCH",
                type: "decimal(18,6)",
                precision: 18,
                scale: 3,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "CurrentAverageGram",
                table: "RII_FISH_BATCH",
                type: "decimal(18,6)",
                precision: 18,
                scale: 3,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8);

            migrationBuilder.AlterColumn<decimal>(
                name: "TotalGram",
                table: "RII_FEEDING_LINE",
                type: "decimal(18,6)",
                precision: 18,
                scale: 3,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8);

            migrationBuilder.AlterColumn<decimal>(
                name: "QtyUnit",
                table: "RII_FEEDING_LINE",
                type: "decimal(18,6)",
                precision: 18,
                scale: 3,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8);

            migrationBuilder.AlterColumn<decimal>(
                name: "GramPerUnit",
                table: "RII_FEEDING_LINE",
                type: "decimal(18,6)",
                precision: 18,
                scale: 3,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8);

            migrationBuilder.AlterColumn<decimal>(
                name: "FeedGram",
                table: "RII_FEEDING_DISTRIBUTION",
                type: "decimal(18,6)",
                precision: 18,
                scale: 3,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8);

            migrationBuilder.AlterColumn<decimal>(
                name: "WindKnot",
                table: "RII_DAILY_WEATHER",
                type: "decimal(18,6)",
                precision: 18,
                scale: 3,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "WaveHeightM",
                table: "RII_DAILY_WEATHER",
                type: "decimal(18,6)",
                precision: 18,
                scale: 3,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "WaterTemperatureSurfaceC",
                table: "RII_DAILY_WEATHER",
                type: "decimal(18,6)",
                precision: 18,
                scale: 3,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "WaterTemperatureDepthC",
                table: "RII_DAILY_WEATHER",
                type: "decimal(18,6)",
                precision: 18,
                scale: 3,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "TurbidityNtu",
                table: "RII_DAILY_WEATHER",
                type: "decimal(18,6)",
                precision: 18,
                scale: 3,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "TemperatureC",
                table: "RII_DAILY_WEATHER",
                type: "decimal(18,6)",
                precision: 18,
                scale: 3,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "SensorHealthScore",
                table: "RII_DAILY_WEATHER",
                type: "decimal(18,6)",
                precision: 18,
                scale: 3,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "SalinityPpt",
                table: "RII_DAILY_WEATHER",
                type: "decimal(18,6)",
                precision: 18,
                scale: 3,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Ph",
                table: "RII_DAILY_WEATHER",
                type: "decimal(18,6)",
                precision: 18,
                scale: 3,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "NitriteMgL",
                table: "RII_DAILY_WEATHER",
                type: "decimal(18,6)",
                precision: 18,
                scale: 3,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "DissolvedOxygenMgL",
                table: "RII_DAILY_WEATHER",
                type: "decimal(18,6)",
                precision: 18,
                scale: 3,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "CurrentSpeedKn",
                table: "RII_DAILY_WEATHER",
                type: "decimal(18,6)",
                precision: 18,
                scale: 3,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "AmmoniaMgL",
                table: "RII_DAILY_WEATHER",
                type: "decimal(18,6)",
                precision: 18,
                scale: 3,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "AlgalBloomIndex",
                table: "RII_DAILY_WEATHER",
                type: "decimal(18,6)",
                precision: 18,
                scale: 3,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "BiomassGram",
                table: "RII_CAGE_WAREHOUSE_TRANSFER_LINE",
                type: "decimal(18,6)",
                precision: 18,
                scale: 3,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8);

            migrationBuilder.AlterColumn<decimal>(
                name: "AverageGram",
                table: "RII_CAGE_WAREHOUSE_TRANSFER_LINE",
                type: "decimal(18,6)",
                precision: 18,
                scale: 3,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8);

            migrationBuilder.AlterColumn<decimal>(
                name: "CapacityGram",
                table: "RII_CAGE",
                type: "decimal(18,6)",
                precision: 18,
                scale: 3,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "WaterTemperatureCelsius",
                table: "RII_BUDGET_WATER_TEMPERATURE",
                type: "decimal(18,3)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8);

            migrationBuilder.AlterColumn<decimal>(
                name: "UnitPrice",
                table: "RII_BUDGET_PLAN_SALES_LINE",
                type: "decimal(18,6)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "SourceUnitPrice",
                table: "RII_BUDGET_PLAN_SALES_LINE",
                type: "decimal(18,6)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "SalesTon",
                table: "RII_BUDGET_PLAN_SALES_LINE",
                type: "decimal(18,6)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8);

            migrationBuilder.AlterColumn<decimal>(
                name: "UnitPriceEuro",
                table: "RII_BUDGET_PLAN_SALES_DISTRIBUTION",
                type: "decimal(18,6)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8);

            migrationBuilder.AlterColumn<decimal>(
                name: "UnitPrice",
                table: "RII_BUDGET_PLAN_SALES_DISTRIBUTION",
                type: "decimal(18,6)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8);

            migrationBuilder.AlterColumn<decimal>(
                name: "UnitGram",
                table: "RII_BUDGET_PLAN_SALES_DISTRIBUTION",
                type: "decimal(18,6)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8);

            migrationBuilder.AlterColumn<decimal>(
                name: "SalesTon",
                table: "RII_BUDGET_PLAN_SALES_DISTRIBUTION",
                type: "decimal(18,6)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8);

            migrationBuilder.AlterColumn<decimal>(
                name: "SalesKg",
                table: "RII_BUDGET_PLAN_SALES_DISTRIBUTION",
                type: "decimal(18,6)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8);

            migrationBuilder.AlterColumn<decimal>(
                name: "ExchangeRate",
                table: "RII_BUDGET_PLAN_SALES_DISTRIBUTION",
                type: "decimal(18,6)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "AmountTry",
                table: "RII_BUDGET_PLAN_SALES_DISTRIBUTION",
                type: "decimal(18,6)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "AmountEuro",
                table: "RII_BUDGET_PLAN_SALES_DISTRIBUTION",
                type: "decimal(18,6)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8);

            migrationBuilder.AlterColumn<decimal>(
                name: "Amount",
                table: "RII_BUDGET_PLAN_SALES_DISTRIBUTION",
                type: "decimal(18,6)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8);

            migrationBuilder.AlterColumn<decimal>(
                name: "MortalityRatePercent",
                table: "RII_BUDGET_PLAN_MORTALITY_LINE",
                type: "decimal(18,6)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8);

            migrationBuilder.AlterColumn<decimal>(
                name: "MortalityKg",
                table: "RII_BUDGET_PLAN_MORTALITY_LINE",
                type: "decimal(18,6)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8);

            migrationBuilder.AlterColumn<decimal>(
                name: "SalesTon",
                table: "RII_BUDGET_PLAN_MONTHLY_PROJECTION",
                type: "decimal(18,6)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8);

            migrationBuilder.AlterColumn<decimal>(
                name: "RawMonthlyGrowthGram",
                table: "RII_BUDGET_PLAN_MONTHLY_PROJECTION",
                type: "decimal(18,6)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8);

            migrationBuilder.AlterColumn<decimal>(
                name: "OpeningBiomassKg",
                table: "RII_BUDGET_PLAN_MONTHLY_PROJECTION",
                type: "decimal(18,6)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8);

            migrationBuilder.AlterColumn<decimal>(
                name: "OpeningAverageGram",
                table: "RII_BUDGET_PLAN_MONTHLY_PROJECTION",
                type: "decimal(18,6)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8);

            migrationBuilder.AlterColumn<decimal>(
                name: "MortalityKg",
                table: "RII_BUDGET_PLAN_MONTHLY_PROJECTION",
                type: "decimal(18,6)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8);

            migrationBuilder.AlterColumn<decimal>(
                name: "MonthlyGrowthGram",
                table: "RII_BUDGET_PLAN_MONTHLY_PROJECTION",
                type: "decimal(18,6)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8);

            migrationBuilder.AlterColumn<decimal>(
                name: "GrowthQualityPercent",
                table: "RII_BUDGET_PLAN_MONTHLY_PROJECTION",
                type: "decimal(18,6)",
                nullable: false,
                defaultValue: 100m,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8,
                oldDefaultValue: 100m);

            migrationBuilder.AlterColumn<decimal>(
                name: "FeedMortalityReductionPercent",
                table: "RII_BUDGET_PLAN_MONTHLY_PROJECTION",
                type: "decimal(18,6)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8);

            migrationBuilder.AlterColumn<decimal>(
                name: "FeedMortalityReductionKg",
                table: "RII_BUDGET_PLAN_MONTHLY_PROJECTION",
                type: "decimal(18,6)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8);

            migrationBuilder.AlterColumn<decimal>(
                name: "FeedKg",
                table: "RII_BUDGET_PLAN_MONTHLY_PROJECTION",
                type: "decimal(18,6)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8);

            migrationBuilder.AlterColumn<decimal>(
                name: "ClosingBiomassKg",
                table: "RII_BUDGET_PLAN_MONTHLY_PROJECTION",
                type: "decimal(18,6)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8);

            migrationBuilder.AlterColumn<decimal>(
                name: "ClosingAverageGram",
                table: "RII_BUDGET_PLAN_MONTHLY_PROJECTION",
                type: "decimal(18,6)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8);

            migrationBuilder.AlterColumn<decimal>(
                name: "UnitPrice",
                table: "RII_BUDGET_PLAN_FISH_PRICE",
                type: "decimal(18,6)",
                precision: 18,
                scale: 6,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8);

            migrationBuilder.AlterColumn<decimal>(
                name: "IncreaseRatePercent",
                table: "RII_BUDGET_PLAN_FISH_PRICE",
                type: "decimal(18,6)",
                precision: 18,
                scale: 6,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8);

            migrationBuilder.AlterColumn<decimal>(
                name: "BiomassKg",
                table: "RII_BUDGET_PLAN_FISH_BATCH_ADJUSTMENT",
                type: "decimal(18,6)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8);

            migrationBuilder.AlterColumn<decimal>(
                name: "AverageGram",
                table: "RII_BUDGET_PLAN_FISH_BATCH_ADJUSTMENT",
                type: "decimal(18,6)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8);

            migrationBuilder.AlterColumn<decimal>(
                name: "InitialUnitCost",
                table: "RII_BUDGET_PLAN_FISH_BATCH",
                type: "decimal(18,6)",
                precision: 18,
                scale: 6,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "InitialSmmAmount",
                table: "RII_BUDGET_PLAN_FISH_BATCH",
                type: "decimal(18,6)",
                precision: 18,
                scale: 6,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "InitialBiomassKg",
                table: "RII_BUDGET_PLAN_FISH_BATCH",
                type: "decimal(18,6)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8);

            migrationBuilder.AlterColumn<decimal>(
                name: "InitialAverageGram",
                table: "RII_BUDGET_PLAN_FISH_BATCH",
                type: "decimal(18,6)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8);

            migrationBuilder.AlterColumn<decimal>(
                name: "MortalityReductionPercent",
                table: "RII_BUDGET_PLAN_FEEDING_LINE",
                type: "decimal(18,6)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8);

            migrationBuilder.AlterColumn<decimal>(
                name: "MortalityReductionKg",
                table: "RII_BUDGET_PLAN_FEEDING_LINE",
                type: "decimal(18,6)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8);

            migrationBuilder.AlterColumn<decimal>(
                name: "FeedKg",
                table: "RII_BUDGET_PLAN_FEEDING_LINE",
                type: "decimal(18,6)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8);

            migrationBuilder.AlterColumn<decimal>(
                name: "FeedAmountRate",
                table: "RII_BUDGET_PLAN_FEEDING_LINE",
                type: "decimal(18,6)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8);

            migrationBuilder.AlterColumn<decimal>(
                name: "ExchangeRate",
                table: "RII_BUDGET_PLAN_EXCHANGE_RATE",
                type: "decimal(18,6)",
                precision: 18,
                scale: 6,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8);

            migrationBuilder.AlterColumn<decimal>(
                name: "MortalityRatePercent",
                table: "RII_BUDGET_MORTALITY_RATE_DEFINITION",
                type: "decimal(18,6)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8);

            migrationBuilder.AlterColumn<decimal>(
                name: "QualityPercent",
                table: "RII_BUDGET_FISH_GROWTH_QUALITY",
                type: "decimal(18,6)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8);

            migrationBuilder.AlterColumn<decimal>(
                name: "TotalGram",
                table: "RII_BUDGET_FISH_GROWTH_PROFILE_LINE",
                type: "decimal(18,8)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8);

            migrationBuilder.AlterColumn<decimal>(
                name: "MonthlyGrowthGram",
                table: "RII_BUDGET_FISH_GROWTH_PROFILE_LINE",
                type: "decimal(18,8)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8);

            migrationBuilder.AlterColumn<decimal>(
                name: "ReductionRatePercent",
                table: "RII_BUDGET_FEED_MORTALITY_RATE",
                type: "decimal(18,6)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8);

            migrationBuilder.AlterColumn<decimal>(
                name: "FeedAmount",
                table: "RII_BUDGET_FEED_CONSUMPTION_RATE",
                type: "decimal(18,6)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8);

            migrationBuilder.AlterColumn<decimal>(
                name: "BiomassGram",
                table: "RII_BATCH_WAREHOUSE_BALANCE",
                type: "decimal(18,6)",
                precision: 18,
                scale: 3,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8);

            migrationBuilder.AlterColumn<decimal>(
                name: "AverageGram",
                table: "RII_BATCH_WAREHOUSE_BALANCE",
                type: "decimal(18,6)",
                precision: 18,
                scale: 3,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8);

            migrationBuilder.AlterColumn<decimal>(
                name: "ToAverageGram",
                table: "RII_BATCH_MOVEMENT",
                type: "decimal(18,6)",
                precision: 18,
                scale: 3,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "SignedBiomassGram",
                table: "RII_BATCH_MOVEMENT",
                type: "decimal(18,6)",
                precision: 18,
                scale: 3,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8);

            migrationBuilder.AlterColumn<decimal>(
                name: "ReportedBiomassGram",
                table: "RII_BATCH_MOVEMENT",
                type: "decimal(18,6)",
                precision: 18,
                scale: 3,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "FromAverageGram",
                table: "RII_BATCH_MOVEMENT",
                type: "decimal(18,6)",
                precision: 18,
                scale: 3,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "FeedGram",
                table: "RII_BATCH_MOVEMENT",
                type: "decimal(18,6)",
                precision: 18,
                scale: 3,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "BiomassGram",
                table: "RII_BATCH_CAGE_BALANCE",
                type: "decimal(18,6)",
                precision: 18,
                scale: 3,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8);

            migrationBuilder.AlterColumn<decimal>(
                name: "AverageGram",
                table: "RII_BATCH_CAGE_BALANCE",
                type: "decimal(18,6)",
                precision: 18,
                scale: 3,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(28,8)",
                oldPrecision: 28,
                oldScale: 8);
        }
    }
}
