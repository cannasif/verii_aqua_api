using aqua_api.Shared.Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace aqua_api.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(AquaDbContext))]
    [Migration("20260424093000_RepairMissingAquaSchemaDrift")]
    public partial class RepairMissingAquaSchemaDrift : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            AddDailyWeatherColumns(migrationBuilder);
            CreateMissingOperationalTables(migrationBuilder);
            AddIndexes(migrationBuilder);
            AddForeignKeys(migrationBuilder);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // This is a repair migration for schema drift between the snapshot and
            // generated SQL script. Do not drop customer data on rollback.
        }

        private static void AddDailyWeatherColumns(MigrationBuilder migrationBuilder)
        {
            AddColumnIfMissing(migrationBuilder, "RII_DailyWeather", "WaterTemperatureSurfaceC", "decimal(18,6) NULL");
            AddColumnIfMissing(migrationBuilder, "RII_DailyWeather", "WaterTemperatureDepthC", "decimal(18,6) NULL");
            AddColumnIfMissing(migrationBuilder, "RII_DailyWeather", "DissolvedOxygenMgL", "decimal(18,6) NULL");
            AddColumnIfMissing(migrationBuilder, "RII_DailyWeather", "SalinityPpt", "decimal(18,6) NULL");
            AddColumnIfMissing(migrationBuilder, "RII_DailyWeather", "Ph", "decimal(18,6) NULL");
            AddColumnIfMissing(migrationBuilder, "RII_DailyWeather", "CurrentSpeedKn", "decimal(18,6) NULL");
            AddColumnIfMissing(migrationBuilder, "RII_DailyWeather", "WaveHeightM", "decimal(18,6) NULL");
            AddColumnIfMissing(migrationBuilder, "RII_DailyWeather", "TurbidityNtu", "decimal(18,6) NULL");
            AddColumnIfMissing(migrationBuilder, "RII_DailyWeather", "AmmoniaMgL", "decimal(18,6) NULL");
            AddColumnIfMissing(migrationBuilder, "RII_DailyWeather", "NitriteMgL", "decimal(18,6) NULL");
            AddColumnIfMissing(migrationBuilder, "RII_DailyWeather", "AlgalBloomIndex", "decimal(18,6) NULL");
            AddColumnIfMissing(migrationBuilder, "RII_DailyWeather", "SensorHealthScore", "decimal(18,6) NULL");
            AddColumnIfMissing(migrationBuilder, "RII_DailyWeather", "SensorRecordedAt", "datetime2(3) NULL");
            AddColumnIfMissing(migrationBuilder, "RII_DailyWeather", "DataSource", "nvarchar(50) NULL");
        }

        private static void CreateMissingOperationalTables(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF OBJECT_ID(N'[dbo].[RII_ComplianceAudit]', N'U') IS NULL
                BEGIN
                    CREATE TABLE [dbo].[RII_ComplianceAudit] (
                        [Id] bigint IDENTITY(1,1) NOT NULL,
                        [ProjectId] bigint NOT NULL,
                        [ProjectCageId] bigint NULL,
                        [FishBatchId] bigint NULL,
                        [AuditDate] datetime2(3) NOT NULL,
                        [StandardCode] nvarchar(50) NOT NULL,
                        [ChecklistCode] nvarchar(50) NULL,
                        [Status] nvarchar(40) NOT NULL,
                        [FindingCount] int NOT NULL,
                        [AuditorName] nvarchar(150) NULL,
                        [Summary] nvarchar(2000) NULL,
                        [NextAuditDate] datetime2(3) NULL,
                        [CreatedDate] datetime2 NOT NULL CONSTRAINT [DF_RII_ComplianceAudit_CreatedDate] DEFAULT(GETDATE()),
                        [UpdatedDate] datetime2 NULL,
                        [DeletedDate] datetime2 NULL,
                        [IsDeleted] bit NOT NULL CONSTRAINT [DF_RII_ComplianceAudit_IsDeleted] DEFAULT(0),
                        [CreatedBy] bigint NULL,
                        [UpdatedBy] bigint NULL,
                        [DeletedBy] bigint NULL,
                        CONSTRAINT [PK_RII_ComplianceAudit] PRIMARY KEY ([Id])
                    );
                END
                """);

            migrationBuilder.Sql(
                """
                IF OBJECT_ID(N'[dbo].[RII_ComplianceCorrectiveAction]', N'U') IS NULL
                BEGIN
                    CREATE TABLE [dbo].[RII_ComplianceCorrectiveAction] (
                        [Id] bigint IDENTITY(1,1) NOT NULL,
                        [ComplianceAuditId] bigint NOT NULL,
                        [ActionCode] nvarchar(50) NOT NULL,
                        [Description] nvarchar(1000) NOT NULL,
                        [Status] nvarchar(40) NOT NULL,
                        [OwnerName] nvarchar(150) NULL,
                        [DueDate] datetime2(3) NULL,
                        [ClosedDate] datetime2(3) NULL,
                        [ClosureNote] nvarchar(1000) NULL,
                        [CreatedDate] datetime2 NOT NULL CONSTRAINT [DF_RII_ComplianceCorrectiveAction_CreatedDate] DEFAULT(GETDATE()),
                        [UpdatedDate] datetime2 NULL,
                        [DeletedDate] datetime2 NULL,
                        [IsDeleted] bit NOT NULL CONSTRAINT [DF_RII_ComplianceCorrectiveAction_IsDeleted] DEFAULT(0),
                        [CreatedBy] bigint NULL,
                        [UpdatedBy] bigint NULL,
                        [DeletedBy] bigint NULL,
                        CONSTRAINT [PK_RII_ComplianceCorrectiveAction] PRIMARY KEY ([Id])
                    );
                END
                """);

            migrationBuilder.Sql(
                """
                IF OBJECT_ID(N'[dbo].[RII_FishHealthEvent]', N'U') IS NULL
                BEGIN
                    CREATE TABLE [dbo].[RII_FishHealthEvent] (
                        [Id] bigint IDENTITY(1,1) NOT NULL,
                        [ProjectId] bigint NOT NULL,
                        [ProjectCageId] bigint NULL,
                        [FishBatchId] bigint NULL,
                        [EventDate] datetime2(3) NOT NULL,
                        [EventType] nvarchar(80) NOT NULL,
                        [Severity] nvarchar(40) NOT NULL,
                        [Status] nvarchar(40) NOT NULL,
                        [AffectedFishCount] int NULL,
                        [AffectedRatioPct] decimal(18,6) NULL,
                        [MortalityCount] int NULL,
                        [IsConfirmed] bit NOT NULL CONSTRAINT [DF_RII_FishHealthEvent_IsConfirmed] DEFAULT(0),
                        [RequiresVeterinaryReview] bit NOT NULL CONSTRAINT [DF_RII_FishHealthEvent_RequiresVeterinaryReview] DEFAULT(0),
                        [VeterinarianName] nvarchar(150) NULL,
                        [Observation] nvarchar(2000) NULL,
                        [RecommendedAction] nvarchar(1000) NULL,
                        [CreatedDate] datetime2 NOT NULL CONSTRAINT [DF_RII_FishHealthEvent_CreatedDate] DEFAULT(GETDATE()),
                        [UpdatedDate] datetime2 NULL,
                        [DeletedDate] datetime2 NULL,
                        [IsDeleted] bit NOT NULL CONSTRAINT [DF_RII_FishHealthEvent_IsDeleted] DEFAULT(0),
                        [CreatedBy] bigint NULL,
                        [UpdatedBy] bigint NULL,
                        [DeletedBy] bigint NULL,
                        CONSTRAINT [PK_RII_FishHealthEvent] PRIMARY KEY ([Id])
                    );
                END
                """);

            migrationBuilder.Sql(
                """
                IF OBJECT_ID(N'[dbo].[RII_FishLabSample]', N'U') IS NULL
                BEGIN
                    CREATE TABLE [dbo].[RII_FishLabSample] (
                        [Id] bigint IDENTITY(1,1) NOT NULL,
                        [ProjectId] bigint NOT NULL,
                        [ProjectCageId] bigint NULL,
                        [FishBatchId] bigint NULL,
                        [FishHealthEventId] bigint NULL,
                        [SampleDate] datetime2(3) NOT NULL,
                        [SampleCode] nvarchar(80) NOT NULL,
                        [SampleType] nvarchar(80) NOT NULL,
                        [LaboratoryName] nvarchar(150) NULL,
                        [RequestedBy] nvarchar(150) NULL,
                        [Note] nvarchar(1000) NULL,
                        [CreatedDate] datetime2 NOT NULL CONSTRAINT [DF_RII_FishLabSample_CreatedDate] DEFAULT(GETDATE()),
                        [UpdatedDate] datetime2 NULL,
                        [DeletedDate] datetime2 NULL,
                        [IsDeleted] bit NOT NULL CONSTRAINT [DF_RII_FishLabSample_IsDeleted] DEFAULT(0),
                        [CreatedBy] bigint NULL,
                        [UpdatedBy] bigint NULL,
                        [DeletedBy] bigint NULL,
                        CONSTRAINT [PK_RII_FishLabSample] PRIMARY KEY ([Id])
                    );
                END
                """);

            migrationBuilder.Sql(
                """
                IF OBJECT_ID(N'[dbo].[RII_FishLabResult]', N'U') IS NULL
                BEGIN
                    CREATE TABLE [dbo].[RII_FishLabResult] (
                        [Id] bigint IDENTITY(1,1) NOT NULL,
                        [FishLabSampleId] bigint NOT NULL,
                        [ResultDate] datetime2(3) NOT NULL,
                        [ResultType] nvarchar(80) NOT NULL,
                        [PathogenName] nvarchar(120) NULL,
                        [ResultValue] nvarchar(120) NULL,
                        [Unit] nvarchar(30) NULL,
                        [IsPositive] bit NOT NULL CONSTRAINT [DF_RII_FishLabResult_IsPositive] DEFAULT(0),
                        [Interpretation] nvarchar(1000) NULL,
                        [RecommendedAction] nvarchar(1000) NULL,
                        [CreatedDate] datetime2 NOT NULL CONSTRAINT [DF_RII_FishLabResult_CreatedDate] DEFAULT(GETDATE()),
                        [UpdatedDate] datetime2 NULL,
                        [DeletedDate] datetime2 NULL,
                        [IsDeleted] bit NOT NULL CONSTRAINT [DF_RII_FishLabResult_IsDeleted] DEFAULT(0),
                        [CreatedBy] bigint NULL,
                        [UpdatedBy] bigint NULL,
                        [DeletedBy] bigint NULL,
                        CONSTRAINT [PK_RII_FishLabResult] PRIMARY KEY ([Id])
                    );
                END
                """);

            migrationBuilder.Sql(
                """
                IF OBJECT_ID(N'[dbo].[RII_FishTreatment]', N'U') IS NULL
                BEGIN
                    CREATE TABLE [dbo].[RII_FishTreatment] (
                        [Id] bigint IDENTITY(1,1) NOT NULL,
                        [ProjectId] bigint NOT NULL,
                        [ProjectCageId] bigint NULL,
                        [FishBatchId] bigint NULL,
                        [FishHealthEventId] bigint NULL,
                        [TreatmentDate] datetime2(3) NOT NULL,
                        [TreatmentType] nvarchar(80) NOT NULL,
                        [MedicationName] nvarchar(120) NOT NULL,
                        [ActiveIngredient] nvarchar(120) NULL,
                        [DoseValue] decimal(18,6) NULL,
                        [DoseUnit] nvarchar(30) NULL,
                        [DurationDays] int NULL,
                        [WithdrawalEndDate] datetime2(3) NULL,
                        [Status] nvarchar(40) NOT NULL,
                        [VeterinarianName] nvarchar(150) NULL,
                        [TreatmentReason] nvarchar(500) NULL,
                        [Note] nvarchar(1000) NULL,
                        [CreatedDate] datetime2 NOT NULL CONSTRAINT [DF_RII_FishTreatment_CreatedDate] DEFAULT(GETDATE()),
                        [UpdatedDate] datetime2 NULL,
                        [DeletedDate] datetime2 NULL,
                        [IsDeleted] bit NOT NULL CONSTRAINT [DF_RII_FishTreatment_IsDeleted] DEFAULT(0),
                        [CreatedBy] bigint NULL,
                        [UpdatedBy] bigint NULL,
                        [DeletedBy] bigint NULL,
                        CONSTRAINT [PK_RII_FishTreatment] PRIMARY KEY ([Id])
                    );
                END
                """);

            migrationBuilder.Sql(
                """
                IF OBJECT_ID(N'[dbo].[RII_ProjectCageDailyKpiSnapshot]', N'U') IS NULL
                BEGIN
                    CREATE TABLE [dbo].[RII_ProjectCageDailyKpiSnapshot] (
                        [Id] bigint IDENTITY(1,1) NOT NULL,
                        [ProjectId] bigint NOT NULL,
                        [ProjectCageId] bigint NOT NULL,
                        [FishBatchId] bigint NOT NULL,
                        [SnapshotDate] datetime2 NOT NULL,
                        [InitialCount] int NOT NULL,
                        [LiveCount] int NOT NULL,
                        [DeadCountPeriod] int NOT NULL,
                        [AverageGram] decimal(18,6) NOT NULL,
                        [BiomassKg] decimal(18,6) NOT NULL,
                        [FeedKgPeriod] decimal(18,6) NOT NULL,
                        [BiomassGainKgPeriod] decimal(18,6) NOT NULL,
                        [SurvivalPct] decimal(18,6) NOT NULL,
                        [MortalityPctPeriod] decimal(18,6) NOT NULL,
                        [Fcr] decimal(18,6) NOT NULL,
                        [Adg] decimal(18,6) NOT NULL,
                        [Sgr] decimal(18,6) NOT NULL,
                        [CapacityUsagePct] decimal(18,6) NOT NULL,
                        [ForecastBiomassKg30Days] decimal(18,6) NOT NULL,
                        [HarvestReadinessScore] decimal(18,6) NOT NULL,
                        [DataQualityScore] decimal(18,6) NOT NULL,
                        [FormulaNote] nvarchar(1000) NULL,
                        [CreatedDate] datetime2 NOT NULL CONSTRAINT [DF_RII_ProjectCageDailyKpiSnapshot_CreatedDate] DEFAULT(GETDATE()),
                        [UpdatedDate] datetime2 NULL,
                        [DeletedDate] datetime2 NULL,
                        [IsDeleted] bit NOT NULL CONSTRAINT [DF_RII_ProjectCageDailyKpiSnapshot_IsDeleted] DEFAULT(0),
                        [CreatedBy] bigint NULL,
                        [UpdatedBy] bigint NULL,
                        [DeletedBy] bigint NULL,
                        CONSTRAINT [PK_RII_ProjectCageDailyKpiSnapshot] PRIMARY KEY ([Id])
                    );
                END
                """);

            migrationBuilder.Sql(
                """
                IF OBJECT_ID(N'[dbo].[RII_WelfareAssessment]', N'U') IS NULL
                BEGIN
                    CREATE TABLE [dbo].[RII_WelfareAssessment] (
                        [Id] bigint IDENTITY(1,1) NOT NULL,
                        [ProjectId] bigint NOT NULL,
                        [ProjectCageId] bigint NULL,
                        [FishBatchId] bigint NULL,
                        [AssessmentDate] datetime2(3) NOT NULL,
                        [WelfareScore] decimal(18,6) NOT NULL,
                        [StockingDensityKgM3] decimal(18,6) NULL,
                        [AppetiteScore] decimal(18,6) NULL,
                        [BehaviorScore] decimal(18,6) NULL,
                        [GillScore] decimal(18,6) NULL,
                        [SkinScore] decimal(18,6) NULL,
                        [FinScore] decimal(18,6) NULL,
                        [AssessedBy] nvarchar(150) NULL,
                        [Observation] nvarchar(1000) NULL,
                        [CreatedDate] datetime2 NOT NULL CONSTRAINT [DF_RII_WelfareAssessment_CreatedDate] DEFAULT(GETDATE()),
                        [UpdatedDate] datetime2 NULL,
                        [DeletedDate] datetime2 NULL,
                        [IsDeleted] bit NOT NULL CONSTRAINT [DF_RII_WelfareAssessment_IsDeleted] DEFAULT(0),
                        [CreatedBy] bigint NULL,
                        [UpdatedBy] bigint NULL,
                        [DeletedBy] bigint NULL,
                        CONSTRAINT [PK_RII_WelfareAssessment] PRIMARY KEY ([Id])
                    );
                END
                """);
        }

        private static void AddIndexes(MigrationBuilder migrationBuilder)
        {
            AddStandardIndexes(migrationBuilder, "RII_ComplianceAudit", "ProjectId", "ProjectCageId", "FishBatchId");
            AddIndexIfMissing(migrationBuilder, "RII_ComplianceCorrectiveAction", "IX_RII_ComplianceCorrectiveAction_ComplianceAuditId", "[ComplianceAuditId]");
            AddAuditIndexes(migrationBuilder, "RII_ComplianceCorrectiveAction");

            AddStandardIndexes(migrationBuilder, "RII_FishHealthEvent", "ProjectId", "ProjectCageId", "FishBatchId");
            AddStandardIndexes(migrationBuilder, "RII_FishLabSample", "ProjectId", "ProjectCageId", "FishBatchId", "FishHealthEventId");
            AddIndexIfMissing(migrationBuilder, "RII_FishLabResult", "IX_RII_FishLabResult_FishLabSampleId", "[FishLabSampleId]");
            AddAuditIndexes(migrationBuilder, "RII_FishLabResult");
            AddStandardIndexes(migrationBuilder, "RII_FishTreatment", "ProjectId", "ProjectCageId", "FishBatchId", "FishHealthEventId");

            AddStandardIndexes(migrationBuilder, "RII_ProjectCageDailyKpiSnapshot", "ProjectId", "ProjectCageId", "FishBatchId");
            AddStandardIndexes(migrationBuilder, "RII_WelfareAssessment", "ProjectId", "ProjectCageId", "FishBatchId");
        }

        private static void AddForeignKeys(MigrationBuilder migrationBuilder)
        {
            AddStandardForeignKeys(migrationBuilder, "RII_ComplianceAudit", includeFishBatch: true, includeProjectCage: true);
            AddForeignKeyIfMissing(migrationBuilder, "RII_ComplianceCorrectiveAction", "FK_RII_ComplianceCorrectiveAction_RII_ComplianceAudit_ComplianceAuditId", "ComplianceAuditId", "RII_ComplianceAudit");
            AddAuditForeignKeys(migrationBuilder, "RII_ComplianceCorrectiveAction");

            AddStandardForeignKeys(migrationBuilder, "RII_FishHealthEvent", includeFishBatch: true, includeProjectCage: true);
            AddStandardForeignKeys(migrationBuilder, "RII_FishLabSample", includeFishBatch: true, includeProjectCage: true);
            AddForeignKeyIfMissing(migrationBuilder, "RII_FishLabSample", "FK_RII_FishLabSample_RII_FishHealthEvent_FishHealthEventId", "FishHealthEventId", "RII_FishHealthEvent");
            AddForeignKeyIfMissing(migrationBuilder, "RII_FishLabResult", "FK_RII_FishLabResult_RII_FishLabSample_FishLabSampleId", "FishLabSampleId", "RII_FishLabSample");
            AddAuditForeignKeys(migrationBuilder, "RII_FishLabResult");
            AddStandardForeignKeys(migrationBuilder, "RII_FishTreatment", includeFishBatch: true, includeProjectCage: true);
            AddForeignKeyIfMissing(migrationBuilder, "RII_FishTreatment", "FK_RII_FishTreatment_RII_FishHealthEvent_FishHealthEventId", "FishHealthEventId", "RII_FishHealthEvent");

            AddStandardForeignKeys(migrationBuilder, "RII_ProjectCageDailyKpiSnapshot", includeFishBatch: true, includeProjectCage: true);
            AddStandardForeignKeys(migrationBuilder, "RII_WelfareAssessment", includeFishBatch: true, includeProjectCage: true);
        }

        private static void AddStandardIndexes(MigrationBuilder migrationBuilder, string table, params string[] domainColumns)
        {
            foreach (var column in domainColumns)
            {
                AddIndexIfMissing(migrationBuilder, table, $"IX_{table}_{column}", $"[{column}]");
            }

            AddAuditIndexes(migrationBuilder, table);
        }

        private static void AddAuditIndexes(MigrationBuilder migrationBuilder, string table)
        {
            AddIndexIfMissing(migrationBuilder, table, $"IX_{table}_CreatedBy", "[CreatedBy]");
            AddIndexIfMissing(migrationBuilder, table, $"IX_{table}_DeletedBy", "[DeletedBy]");
            AddIndexIfMissing(migrationBuilder, table, $"IX_{table}_UpdatedBy", "[UpdatedBy]");
        }

        private static void AddStandardForeignKeys(MigrationBuilder migrationBuilder, string table, bool includeFishBatch, bool includeProjectCage)
        {
            AddForeignKeyIfMissing(migrationBuilder, table, $"FK_{table}_RII_Project_ProjectId", "ProjectId", "RII_Project");

            if (includeProjectCage)
            {
                AddForeignKeyIfMissing(migrationBuilder, table, $"FK_{table}_RII_ProjectCage_ProjectCageId", "ProjectCageId", "RII_ProjectCage");
            }

            if (includeFishBatch)
            {
                AddForeignKeyIfMissing(migrationBuilder, table, $"FK_{table}_RII_FishBatch_FishBatchId", "FishBatchId", "RII_FishBatch");
            }

            AddAuditForeignKeys(migrationBuilder, table);
        }

        private static void AddAuditForeignKeys(MigrationBuilder migrationBuilder, string table)
        {
            AddForeignKeyIfMissing(migrationBuilder, table, $"FK_{table}_RII_USERS_CreatedBy", "CreatedBy", "RII_USERS");
            AddForeignKeyIfMissing(migrationBuilder, table, $"FK_{table}_RII_USERS_DeletedBy", "DeletedBy", "RII_USERS");
            AddForeignKeyIfMissing(migrationBuilder, table, $"FK_{table}_RII_USERS_UpdatedBy", "UpdatedBy", "RII_USERS");
        }

        private static void AddColumnIfMissing(MigrationBuilder migrationBuilder, string table, string column, string definition)
        {
            migrationBuilder.Sql(
                $"""
                IF OBJECT_ID(N'[dbo].[{table}]', N'U') IS NOT NULL
                   AND COL_LENGTH('dbo.{table}', '{column}') IS NULL
                BEGIN
                    ALTER TABLE [dbo].[{table}] ADD [{column}] {definition};
                END
                """);
        }

        private static void AddIndexIfMissing(MigrationBuilder migrationBuilder, string table, string indexName, string columns)
        {
            migrationBuilder.Sql(
                $"""
                IF OBJECT_ID(N'[dbo].[{table}]', N'U') IS NOT NULL
                   AND NOT EXISTS (
                       SELECT 1
                       FROM sys.indexes
                       WHERE name = N'{indexName}'
                         AND object_id = OBJECT_ID(N'[dbo].[{table}]')
                   )
                BEGIN
                    CREATE INDEX [{indexName}] ON [dbo].[{table}] ({columns});
                END
                """);
        }

        private static void AddForeignKeyIfMissing(MigrationBuilder migrationBuilder, string table, string foreignKeyName, string column, string principalTable)
        {
            migrationBuilder.Sql(
                $"""
                IF OBJECT_ID(N'[dbo].[{table}]', N'U') IS NOT NULL
                   AND COL_LENGTH('dbo.{table}', '{column}') IS NOT NULL
                   AND OBJECT_ID(N'[dbo].[{principalTable}]', N'U') IS NOT NULL
                   AND NOT EXISTS (
                       SELECT 1
                       FROM sys.foreign_keys
                       WHERE name = N'{foreignKeyName}'
                         AND parent_object_id = OBJECT_ID(N'[dbo].[{table}]')
                   )
                BEGIN
                    ALTER TABLE [dbo].[{table}] WITH NOCHECK
                    ADD CONSTRAINT [{foreignKeyName}]
                    FOREIGN KEY ([{column}]) REFERENCES [dbo].[{principalTable}] ([Id]);
                END
                """);
        }
    }
}
