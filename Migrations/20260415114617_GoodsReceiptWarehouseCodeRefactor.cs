using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace aqua_api.Migrations
{
    /// <inheritdoc />
    public partial class GoodsReceiptWarehouseCodeRefactor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF COL_LENGTH('dbo.RII_GoodsReceipt', 'WarehouseId') IS NOT NULL
                BEGIN
                    ALTER TABLE [dbo].[RII_GoodsReceipt] DROP COLUMN [WarehouseId];
                END
                """);

            migrationBuilder.Sql(
                """
                IF COL_LENGTH('dbo.RII_GoodsReceipt', 'WarehouseBranchCode') IS NULL
                BEGIN
                    ALTER TABLE [dbo].[RII_GoodsReceipt] ADD [WarehouseBranchCode] int NULL;
                END
                """);

            migrationBuilder.Sql(
                """
                IF COL_LENGTH('dbo.RII_GoodsReceipt', 'WarehouseCode') IS NULL
                BEGIN
                    ALTER TABLE [dbo].[RII_GoodsReceipt] ADD [WarehouseCode] smallint NULL;
                END
                """);

            migrationBuilder.Sql(
                """
                IF NOT EXISTS (
                    SELECT 1
                    FROM sys.indexes
                    WHERE name = 'IX_RII_GoodsReceipt_WarehouseCode_BranchCode'
                      AND object_id = OBJECT_ID(N'[dbo].[RII_GoodsReceipt]')
                )
                BEGIN
                    CREATE INDEX [IX_RII_GoodsReceipt_WarehouseCode_BranchCode]
                    ON [dbo].[RII_GoodsReceipt] ([WarehouseCode], [WarehouseBranchCode]);
                END
                """);

            migrationBuilder.Sql(
                """
                IF OBJECT_ID(N'[dbo].[RII_Warehouse]', N'U') IS NULL
                BEGIN
                    CREATE TABLE [dbo].[RII_Warehouse]
                    (
                        [Id] bigint IDENTITY(1,1) NOT NULL,
                        [ErpWarehouseCode] smallint NOT NULL,
                        [WarehouseName] nvarchar(150) NOT NULL,
                        [CustomerCode] nvarchar(25) NULL,
                        [BranchCode] int NOT NULL,
                        [IsLocked] bit NOT NULL CONSTRAINT [DF_RII_Warehouse_IsLocked] DEFAULT(0),
                        [AllowNegativeBalance] bit NOT NULL CONSTRAINT [DF_RII_Warehouse_AllowNegativeBalance] DEFAULT(0),
                        [LastSyncedAt] datetime2 NULL,
                        [CreatedDate] datetime2 NOT NULL CONSTRAINT [DF_RII_Warehouse_CreatedDate] DEFAULT(GETDATE()),
                        [UpdatedDate] datetime2 NULL,
                        [DeletedDate] datetime2 NULL,
                        [IsDeleted] bit NOT NULL CONSTRAINT [DF_RII_Warehouse_IsDeleted] DEFAULT(0),
                        [CreatedBy] bigint NULL,
                        [UpdatedBy] bigint NULL,
                        [DeletedBy] bigint NULL,
                        CONSTRAINT [PK_RII_Warehouse] PRIMARY KEY ([Id])
                    );
                END
                """);

            migrationBuilder.Sql(
                """
                IF NOT EXISTS (
                    SELECT 1
                    FROM sys.indexes
                    WHERE name = 'UX_RII_Warehouse_ErpWarehouseCode_BranchCode'
                      AND object_id = OBJECT_ID(N'[dbo].[RII_Warehouse]')
                )
                BEGIN
                    CREATE UNIQUE INDEX [UX_RII_Warehouse_ErpWarehouseCode_BranchCode]
                    ON [dbo].[RII_Warehouse] ([ErpWarehouseCode], [BranchCode]);
                END
                """);

            migrationBuilder.Sql(
                """
                IF NOT EXISTS (
                    SELECT 1
                    FROM sys.indexes
                    WHERE name = 'IX_RII_Warehouse_WarehouseName'
                      AND object_id = OBJECT_ID(N'[dbo].[RII_Warehouse]')
                )
                BEGIN
                    CREATE INDEX [IX_RII_Warehouse_WarehouseName]
                    ON [dbo].[RII_Warehouse] ([WarehouseName]);
                END
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF EXISTS (
                    SELECT 1
                    FROM sys.indexes
                    WHERE name = 'IX_RII_GoodsReceipt_WarehouseCode_BranchCode'
                      AND object_id = OBJECT_ID(N'[dbo].[RII_GoodsReceipt]')
                )
                BEGIN
                    DROP INDEX [IX_RII_GoodsReceipt_WarehouseCode_BranchCode] ON [dbo].[RII_GoodsReceipt];
                END
                """);

            migrationBuilder.Sql(
                """
                IF COL_LENGTH('dbo.RII_GoodsReceipt', 'WarehouseBranchCode') IS NOT NULL
                BEGIN
                    ALTER TABLE [dbo].[RII_GoodsReceipt] DROP COLUMN [WarehouseBranchCode];
                END
                """);

            migrationBuilder.Sql(
                """
                IF COL_LENGTH('dbo.RII_GoodsReceipt', 'WarehouseCode') IS NOT NULL
                BEGIN
                    ALTER TABLE [dbo].[RII_GoodsReceipt] DROP COLUMN [WarehouseCode];
                END
                """);

            migrationBuilder.Sql(
                """
                IF COL_LENGTH('dbo.RII_GoodsReceipt', 'WarehouseId') IS NULL
                BEGIN
                    ALTER TABLE [dbo].[RII_GoodsReceipt] ADD [WarehouseId] bigint NULL;
                END
                """);

            migrationBuilder.Sql(
                """
                IF OBJECT_ID(N'[dbo].[RII_Warehouse]', N'U') IS NOT NULL
                BEGIN
                    DROP TABLE [dbo].[RII_Warehouse];
                END
                """);
        }
    }
}
