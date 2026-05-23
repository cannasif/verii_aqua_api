using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace aqua_api.Migrations
{
    /// <inheritdoc />
    public partial class RepairMissingCageWarehouseMappingTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
IF OBJECT_ID(N'[dbo].[RII_CageWarehouseMapping]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[RII_CageWarehouseMapping] (
        [Id] bigint IDENTITY(1,1) NOT NULL,
        [CageId] bigint NOT NULL,
        [WarehouseId] bigint NOT NULL,
        [IsActive] bit NOT NULL CONSTRAINT [DF_RII_CageWarehouseMapping_IsActive] DEFAULT (CONVERT(bit, 1)),
        [Note] nvarchar(500) NULL,
        [CreatedDate] datetime2 NOT NULL CONSTRAINT [DF_RII_CageWarehouseMapping_CreatedDate] DEFAULT (GETDATE()),
        [UpdatedDate] datetime2 NULL,
        [DeletedDate] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        [CreatedBy] bigint NULL,
        [UpdatedBy] bigint NULL,
        [DeletedBy] bigint NULL,
        CONSTRAINT [PK_RII_CageWarehouseMapping] PRIMARY KEY ([Id])
    );
END
""");

            migrationBuilder.Sql("""
IF OBJECT_ID(N'[dbo].[RII_CageWarehouseMapping]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_Cage]', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE [name] = N'FK_RII_CageWarehouseMapping_RII_Cage_CageId')
BEGIN
    ALTER TABLE [dbo].[RII_CageWarehouseMapping] WITH CHECK
    ADD CONSTRAINT [FK_RII_CageWarehouseMapping_RII_Cage_CageId]
    FOREIGN KEY ([CageId]) REFERENCES [dbo].[RII_Cage] ([Id]);
END
""");

            migrationBuilder.Sql("""
IF OBJECT_ID(N'[dbo].[RII_CageWarehouseMapping]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_Warehouse]', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE [name] = N'FK_RII_CageWarehouseMapping_RII_Warehouse_WarehouseId')
BEGIN
    ALTER TABLE [dbo].[RII_CageWarehouseMapping] WITH CHECK
    ADD CONSTRAINT [FK_RII_CageWarehouseMapping_RII_Warehouse_WarehouseId]
    FOREIGN KEY ([WarehouseId]) REFERENCES [dbo].[RII_Warehouse] ([Id]);
END
""");

            migrationBuilder.Sql("""
IF OBJECT_ID(N'[dbo].[RII_CageWarehouseMapping]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_USERS]', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE [name] = N'FK_RII_CageWarehouseMapping_RII_USERS_CreatedBy')
BEGIN
    ALTER TABLE [dbo].[RII_CageWarehouseMapping] WITH CHECK
    ADD CONSTRAINT [FK_RII_CageWarehouseMapping_RII_USERS_CreatedBy]
    FOREIGN KEY ([CreatedBy]) REFERENCES [dbo].[RII_USERS] ([Id]);
END
""");

            migrationBuilder.Sql("""
IF OBJECT_ID(N'[dbo].[RII_CageWarehouseMapping]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_USERS]', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE [name] = N'FK_RII_CageWarehouseMapping_RII_USERS_UpdatedBy')
BEGIN
    ALTER TABLE [dbo].[RII_CageWarehouseMapping] WITH CHECK
    ADD CONSTRAINT [FK_RII_CageWarehouseMapping_RII_USERS_UpdatedBy]
    FOREIGN KEY ([UpdatedBy]) REFERENCES [dbo].[RII_USERS] ([Id]);
END
""");

            migrationBuilder.Sql("""
IF OBJECT_ID(N'[dbo].[RII_CageWarehouseMapping]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[RII_USERS]', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE [name] = N'FK_RII_CageWarehouseMapping_RII_USERS_DeletedBy')
BEGIN
    ALTER TABLE [dbo].[RII_CageWarehouseMapping] WITH CHECK
    ADD CONSTRAINT [FK_RII_CageWarehouseMapping_RII_USERS_DeletedBy]
    FOREIGN KEY ([DeletedBy]) REFERENCES [dbo].[RII_USERS] ([Id]);
END
""");

            migrationBuilder.Sql("""
IF OBJECT_ID(N'[dbo].[RII_CageWarehouseMapping]', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE [name] = N'IX_RII_CageWarehouseMapping_CreatedBy' AND [object_id] = OBJECT_ID(N'[dbo].[RII_CageWarehouseMapping]'))
BEGIN
    CREATE INDEX [IX_RII_CageWarehouseMapping_CreatedBy] ON [dbo].[RII_CageWarehouseMapping] ([CreatedBy]);
END
""");

            migrationBuilder.Sql("""
IF OBJECT_ID(N'[dbo].[RII_CageWarehouseMapping]', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE [name] = N'IX_RII_CageWarehouseMapping_UpdatedBy' AND [object_id] = OBJECT_ID(N'[dbo].[RII_CageWarehouseMapping]'))
BEGIN
    CREATE INDEX [IX_RII_CageWarehouseMapping_UpdatedBy] ON [dbo].[RII_CageWarehouseMapping] ([UpdatedBy]);
END
""");

            migrationBuilder.Sql("""
IF OBJECT_ID(N'[dbo].[RII_CageWarehouseMapping]', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE [name] = N'IX_RII_CageWarehouseMapping_DeletedBy' AND [object_id] = OBJECT_ID(N'[dbo].[RII_CageWarehouseMapping]'))
BEGIN
    CREATE INDEX [IX_RII_CageWarehouseMapping_DeletedBy] ON [dbo].[RII_CageWarehouseMapping] ([DeletedBy]);
END
""");

            migrationBuilder.Sql("""
IF OBJECT_ID(N'[dbo].[RII_CageWarehouseMapping]', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE [name] = N'IX_RII_CageWarehouseMapping_WarehouseId' AND [object_id] = OBJECT_ID(N'[dbo].[RII_CageWarehouseMapping]'))
BEGIN
    CREATE INDEX [IX_RII_CageWarehouseMapping_WarehouseId] ON [dbo].[RII_CageWarehouseMapping] ([WarehouseId]);
END
""");

            migrationBuilder.Sql("""
IF OBJECT_ID(N'[dbo].[RII_CageWarehouseMapping]', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE [name] = N'UX_RII_CageWarehouseMapping_Cage_Active' AND [object_id] = OBJECT_ID(N'[dbo].[RII_CageWarehouseMapping]'))
BEGIN
    CREATE UNIQUE INDEX [UX_RII_CageWarehouseMapping_Cage_Active]
    ON [dbo].[RII_CageWarehouseMapping] ([CageId])
    WHERE [IsDeleted] = 0 AND [IsActive] = 1;
END
""");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Intentionally left empty. This migration repairs schema drift on live databases.
        }
    }
}
