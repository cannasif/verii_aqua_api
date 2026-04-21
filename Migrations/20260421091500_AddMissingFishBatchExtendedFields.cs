using aqua_api.Shared.Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace aqua_api.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(AquaDbContext))]
    [Migration("20260421091500_AddMissingFishBatchExtendedFields")]
    public partial class AddMissingFishBatchExtendedFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF COL_LENGTH('dbo.RII_FishBatch', 'SupplierId') IS NULL
                BEGIN
                    ALTER TABLE [dbo].[RII_FishBatch] ADD [SupplierId] bigint NULL;
                END
                """);

            migrationBuilder.Sql(
                """
                IF COL_LENGTH('dbo.RII_FishBatch', 'SupplierLotCode') IS NULL
                BEGIN
                    ALTER TABLE [dbo].[RII_FishBatch] ADD [SupplierLotCode] nvarchar(100) NULL;
                END
                """);

            migrationBuilder.Sql(
                """
                IF COL_LENGTH('dbo.RII_FishBatch', 'HatcheryName') IS NULL
                BEGIN
                    ALTER TABLE [dbo].[RII_FishBatch] ADD [HatcheryName] nvarchar(150) NULL;
                END
                """);

            migrationBuilder.Sql(
                """
                IF COL_LENGTH('dbo.RII_FishBatch', 'OriginCountryCode') IS NULL
                BEGIN
                    ALTER TABLE [dbo].[RII_FishBatch] ADD [OriginCountryCode] nvarchar(10) NULL;
                END
                """);

            migrationBuilder.Sql(
                """
                IF COL_LENGTH('dbo.RII_FishBatch', 'StrainCode') IS NULL
                BEGIN
                    ALTER TABLE [dbo].[RII_FishBatch] ADD [StrainCode] nvarchar(50) NULL;
                END
                """);

            migrationBuilder.Sql(
                """
                IF COL_LENGTH('dbo.RII_FishBatch', 'GenerationCode') IS NULL
                BEGIN
                    ALTER TABLE [dbo].[RII_FishBatch] ADD [GenerationCode] nvarchar(50) NULL;
                END
                """);

            migrationBuilder.Sql(
                """
                IF COL_LENGTH('dbo.RII_FishBatch', 'BroodstockCode') IS NULL
                BEGIN
                    ALTER TABLE [dbo].[RII_FishBatch] ADD [BroodstockCode] nvarchar(50) NULL;
                END
                """);

            migrationBuilder.Sql(
                """
                IF COL_LENGTH('dbo.RII_FishBatch', 'IsVaccinated') IS NULL
                BEGIN
                    ALTER TABLE [dbo].[RII_FishBatch] ADD [IsVaccinated] bit NOT NULL CONSTRAINT [DF_RII_FishBatch_IsVaccinated] DEFAULT(0);
                END
                """);

            migrationBuilder.Sql(
                """
                IF COL_LENGTH('dbo.RII_FishBatch', 'VaccinationDate') IS NULL
                BEGIN
                    ALTER TABLE [dbo].[RII_FishBatch] ADD [VaccinationDate] datetime2(3) NULL;
                END
                """);

            migrationBuilder.Sql(
                """
                IF COL_LENGTH('dbo.RII_FishBatch', 'VaccinationNote') IS NULL
                BEGIN
                    ALTER TABLE [dbo].[RII_FishBatch] ADD [VaccinationNote] nvarchar(500) NULL;
                END
                """);

            migrationBuilder.Sql(
                """
                IF COL_LENGTH('dbo.RII_FishBatch', 'TreatmentHistoryNote') IS NULL
                BEGIN
                    ALTER TABLE [dbo].[RII_FishBatch] ADD [TreatmentHistoryNote] nvarchar(1000) NULL;
                END
                """);

            migrationBuilder.Sql(
                """
                IF COL_LENGTH('dbo.RII_FishBatch', 'TargetHarvestAverageGram') IS NULL
                BEGIN
                    ALTER TABLE [dbo].[RII_FishBatch] ADD [TargetHarvestAverageGram] decimal(18,3) NULL;
                END
                """);

            migrationBuilder.Sql(
                """
                IF COL_LENGTH('dbo.RII_FishBatch', 'TargetHarvestDate') IS NULL
                BEGIN
                    ALTER TABLE [dbo].[RII_FishBatch] ADD [TargetHarvestDate] datetime2(3) NULL;
                END
                """);

            migrationBuilder.Sql(
                """
                IF COL_LENGTH('dbo.RII_FishBatch', 'TargetHarvestClass') IS NULL
                BEGIN
                    ALTER TABLE [dbo].[RII_FishBatch] ADD [TargetHarvestClass] nvarchar(50) NULL;
                END
                """);

            migrationBuilder.Sql(
                """
                IF COL_LENGTH('dbo.RII_FishBatch', 'QualityGrade') IS NULL
                BEGIN
                    ALTER TABLE [dbo].[RII_FishBatch] ADD [QualityGrade] nvarchar(50) NULL;
                END
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF COL_LENGTH('dbo.RII_FishBatch', 'QualityGrade') IS NOT NULL
                BEGIN
                    ALTER TABLE [dbo].[RII_FishBatch] DROP COLUMN [QualityGrade];
                END
                """);

            migrationBuilder.Sql(
                """
                IF COL_LENGTH('dbo.RII_FishBatch', 'TargetHarvestClass') IS NOT NULL
                BEGIN
                    ALTER TABLE [dbo].[RII_FishBatch] DROP COLUMN [TargetHarvestClass];
                END
                """);

            migrationBuilder.Sql(
                """
                IF COL_LENGTH('dbo.RII_FishBatch', 'TargetHarvestDate') IS NOT NULL
                BEGIN
                    ALTER TABLE [dbo].[RII_FishBatch] DROP COLUMN [TargetHarvestDate];
                END
                """);

            migrationBuilder.Sql(
                """
                IF COL_LENGTH('dbo.RII_FishBatch', 'TargetHarvestAverageGram') IS NOT NULL
                BEGIN
                    ALTER TABLE [dbo].[RII_FishBatch] DROP COLUMN [TargetHarvestAverageGram];
                END
                """);

            migrationBuilder.Sql(
                """
                IF COL_LENGTH('dbo.RII_FishBatch', 'TreatmentHistoryNote') IS NOT NULL
                BEGIN
                    ALTER TABLE [dbo].[RII_FishBatch] DROP COLUMN [TreatmentHistoryNote];
                END
                """);

            migrationBuilder.Sql(
                """
                IF COL_LENGTH('dbo.RII_FishBatch', 'VaccinationNote') IS NOT NULL
                BEGIN
                    ALTER TABLE [dbo].[RII_FishBatch] DROP COLUMN [VaccinationNote];
                END
                """);

            migrationBuilder.Sql(
                """
                IF COL_LENGTH('dbo.RII_FishBatch', 'VaccinationDate') IS NOT NULL
                BEGIN
                    ALTER TABLE [dbo].[RII_FishBatch] DROP COLUMN [VaccinationDate];
                END
                """);

            migrationBuilder.Sql(
                """
                IF COL_LENGTH('dbo.RII_FishBatch', 'IsVaccinated') IS NOT NULL
                BEGIN
                    ALTER TABLE [dbo].[RII_FishBatch] DROP CONSTRAINT IF EXISTS [DF_RII_FishBatch_IsVaccinated];
                    ALTER TABLE [dbo].[RII_FishBatch] DROP COLUMN [IsVaccinated];
                END
                """);

            migrationBuilder.Sql(
                """
                IF COL_LENGTH('dbo.RII_FishBatch', 'BroodstockCode') IS NOT NULL
                BEGIN
                    ALTER TABLE [dbo].[RII_FishBatch] DROP COLUMN [BroodstockCode];
                END
                """);

            migrationBuilder.Sql(
                """
                IF COL_LENGTH('dbo.RII_FishBatch', 'GenerationCode') IS NOT NULL
                BEGIN
                    ALTER TABLE [dbo].[RII_FishBatch] DROP COLUMN [GenerationCode];
                END
                """);

            migrationBuilder.Sql(
                """
                IF COL_LENGTH('dbo.RII_FishBatch', 'StrainCode') IS NOT NULL
                BEGIN
                    ALTER TABLE [dbo].[RII_FishBatch] DROP COLUMN [StrainCode];
                END
                """);

            migrationBuilder.Sql(
                """
                IF COL_LENGTH('dbo.RII_FishBatch', 'OriginCountryCode') IS NOT NULL
                BEGIN
                    ALTER TABLE [dbo].[RII_FishBatch] DROP COLUMN [OriginCountryCode];
                END
                """);

            migrationBuilder.Sql(
                """
                IF COL_LENGTH('dbo.RII_FishBatch', 'HatcheryName') IS NOT NULL
                BEGIN
                    ALTER TABLE [dbo].[RII_FishBatch] DROP COLUMN [HatcheryName];
                END
                """);

            migrationBuilder.Sql(
                """
                IF COL_LENGTH('dbo.RII_FishBatch', 'SupplierLotCode') IS NOT NULL
                BEGIN
                    ALTER TABLE [dbo].[RII_FishBatch] DROP COLUMN [SupplierLotCode];
                END
                """);

            migrationBuilder.Sql(
                """
                IF COL_LENGTH('dbo.RII_FishBatch', 'SupplierId') IS NOT NULL
                BEGIN
                    ALTER TABLE [dbo].[RII_FishBatch] DROP COLUMN [SupplierId];
                END
                """);
        }
    }
}
