using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace aqua_api.Migrations
{
    /// <inheritdoc />
    public partial class RepairLegacyBatchMovementTypeConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            NormalizeMovementTypeConstraint(migrationBuilder);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // This migration repairs unknown legacy constraint names. Their exact
            // previous state cannot be restored safely, so keep the valid schema.
            NormalizeMovementTypeConstraint(migrationBuilder);
        }

        private static void NormalizeMovementTypeConstraint(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DECLARE @ConstraintName sysname;
                DECLARE @DropSql nvarchar(max);

                WHILE 1 = 1
                BEGIN
                    SELECT TOP (1) @ConstraintName = cc.[name]
                    FROM sys.check_constraints AS cc
                    WHERE cc.[parent_object_id] = OBJECT_ID(N'[dbo].[RII_BATCH_MOVEMENT]')
                      AND cc.[definition] LIKE N'%MovementType%';

                    IF @ConstraintName IS NULL
                        BREAK;

                    SET @DropSql =
                        N'ALTER TABLE [dbo].[RII_BATCH_MOVEMENT] DROP CONSTRAINT '
                        + QUOTENAME(@ConstraintName);
                    EXEC sys.sp_executesql @DropSql;

                    SET @ConstraintName = NULL;
                END;

                ALTER TABLE [dbo].[RII_BATCH_MOVEMENT]
                    ADD CONSTRAINT [CK_RII_BATCH_MOVEMENT_MOVEMENT_TYPE]
                    CHECK ([MovementType] IN (0,1,2,3,4,5,6,7,8,9,10));
                """);
        }
    }
}
