using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace aqua_api.Migrations
{
    /// <inheritdoc />
    public partial class AddFishGrowthModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Existing customer databases may already have a different movement
            // constraint name (or no constraint at all). Drop only the exact
            // constraint when it exists so the pending migration remains safe.
            migrationBuilder.Sql(@"
IF EXISTS (
    SELECT 1
    FROM sys.check_constraints
    WHERE name = N'CK_RII_BATCH_MOVEMENT_MOVEMENT_TYPE'
      AND parent_object_id = OBJECT_ID(N'dbo.RII_BATCH_MOVEMENT')
)
BEGIN
    ALTER TABLE [dbo].[RII_BATCH_MOVEMENT]
        DROP CONSTRAINT [CK_RII_BATCH_MOVEMENT_MOVEMENT_TYPE];
END");

            migrationBuilder.CreateTable(
                name: "RII_FISH_GROWTH",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectId = table.Column<long>(type: "bigint", nullable: false),
                    ProjectCageId = table.Column<long>(type: "bigint", nullable: false),
                    FishBatchId = table.Column<long>(type: "bigint", nullable: false),
                    GrowthDate = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
                    GrowthYear = table.Column<int>(type: "int", nullable: false),
                    GrowthMonth = table.Column<byte>(type: "tinyint", nullable: false),
                    FishCount = table.Column<int>(type: "int", nullable: false),
                    PreviousAverageGram = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 3, nullable: false),
                    GrowthGram = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 3, nullable: false),
                    NewAverageGram = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 3, nullable: false),
                    PreviousBiomassGram = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 3, nullable: false),
                    NewBiomassGram = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 3, nullable: false),
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
                    table.PrimaryKey("PK_RII_FISH_GROWTH", x => x.Id);
                    table.CheckConstraint("CK_RII_FISH_GROWTH_MONTH", "[GrowthMonth] BETWEEN 1 AND 12");
                    table.CheckConstraint("CK_RII_FISH_GROWTH_VALUES", "[FishCount] > 0 AND [PreviousAverageGram] > 0 AND [GrowthGram] > 0 AND [NewAverageGram] > [PreviousAverageGram] AND [NewBiomassGram] > [PreviousBiomassGram]");
                    table.ForeignKey(
                        name: "FK_RII_FISH_GROWTH_RII_FISH_BATCH_FishBatchId",
                        column: x => x.FishBatchId,
                        principalTable: "RII_FISH_BATCH",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_FISH_GROWTH_RII_PROJECT_CAGE_ProjectCageId",
                        column: x => x.ProjectCageId,
                        principalTable: "RII_PROJECT_CAGE",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_FISH_GROWTH_RII_PROJECT_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "RII_PROJECT",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_FISH_GROWTH_RII_USERS_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "RII_USERS",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_FISH_GROWTH_RII_USERS_DeletedBy",
                        column: x => x.DeletedBy,
                        principalTable: "RII_USERS",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_FISH_GROWTH_RII_USERS_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "RII_USERS",
                        principalColumn: "Id");
                });

            migrationBuilder.AddCheckConstraint(
                name: "CK_RII_BATCH_MOVEMENT_MOVEMENT_TYPE",
                table: "RII_BATCH_MOVEMENT",
                sql: "[MovementType] IN (0,1,2,3,4,5,6,7,8,9,10)");

            migrationBuilder.CreateIndex(
                name: "IX_RII_FISH_GROWTH_CreatedBy",
                table: "RII_FISH_GROWTH",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_RII_FISH_GROWTH_DeletedBy",
                table: "RII_FISH_GROWTH",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_RII_FISH_GROWTH_FishBatchId",
                table: "RII_FISH_GROWTH",
                column: "FishBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_RII_FISH_GROWTH_PROJECT_DATE",
                table: "RII_FISH_GROWTH",
                columns: new[] { "ProjectId", "GrowthDate" });

            migrationBuilder.CreateIndex(
                name: "IX_RII_FISH_GROWTH_UpdatedBy",
                table: "RII_FISH_GROWTH",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "UX_RII_FISH_GROWTH_CAGE_BATCH_PERIOD_ACTIVE",
                table: "RII_FISH_GROWTH",
                columns: new[] { "ProjectCageId", "FishBatchId", "GrowthYear", "GrowthMonth" },
                unique: true,
                filter: "[IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RII_FISH_GROWTH");

            migrationBuilder.Sql(@"
IF EXISTS (
    SELECT 1
    FROM sys.check_constraints
    WHERE name = N'CK_RII_BATCH_MOVEMENT_MOVEMENT_TYPE'
      AND parent_object_id = OBJECT_ID(N'dbo.RII_BATCH_MOVEMENT')
)
BEGIN
    ALTER TABLE [dbo].[RII_BATCH_MOVEMENT]
        DROP CONSTRAINT [CK_RII_BATCH_MOVEMENT_MOVEMENT_TYPE];
END");

            migrationBuilder.AddCheckConstraint(
                name: "CK_RII_BATCH_MOVEMENT_MOVEMENT_TYPE",
                table: "RII_BATCH_MOVEMENT",
                sql: "[MovementType] IN (0,1,2,3,4,5,6,7,8,9)");
        }
    }
}
