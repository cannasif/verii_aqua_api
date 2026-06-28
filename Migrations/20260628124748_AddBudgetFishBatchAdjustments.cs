using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace aqua_api.Migrations
{
    /// <inheritdoc />
    public partial class AddBudgetFishBatchAdjustments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RII_BUDGET_PlanFishBatchAdjustment",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BudgetPlanId = table.Column<long>(type: "bigint", nullable: false),
                    BudgetPlanFishBatchId = table.Column<long>(type: "bigint", nullable: false),
                    AdjustmentType = table.Column<byte>(type: "tinyint", nullable: false),
                    LiveCount = table.Column<int>(type: "int", nullable: false),
                    AverageGram = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    BiomassKg = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
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
                    table.PrimaryKey("PK_RII_BUDGET_PlanFishBatchAdjustment", x => x.Id);
                    table.CheckConstraint("CK_RII_BUDGET_PlanFishBatchAdjustment_Positive", "[LiveCount] > 0 AND [AverageGram] >= 0 AND [BiomassKg] >= 0");
                    table.CheckConstraint("CK_RII_BUDGET_PlanFishBatchAdjustment_Type", "[AdjustmentType] IN (0,1)");
                    table.ForeignKey(
                        name: "FK_RII_BUDGET_PlanFishBatchAdjustment_RII_BUDGET_PlanFishBatch_BudgetPlanFishBatchId",
                        column: x => x.BudgetPlanFishBatchId,
                        principalTable: "RII_BUDGET_PlanFishBatch",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_BUDGET_PlanFishBatchAdjustment_RII_BUDGET_Plan_BudgetPlanId",
                        column: x => x.BudgetPlanId,
                        principalTable: "RII_BUDGET_Plan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RII_BUDGET_PlanFishBatchAdjustment_RII_USERS_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "RII_USERS",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_BUDGET_PlanFishBatchAdjustment_RII_USERS_DeletedBy",
                        column: x => x.DeletedBy,
                        principalTable: "RII_USERS",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_BUDGET_PlanFishBatchAdjustment_RII_USERS_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "RII_USERS",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_RII_BUDGET_PlanFishBatchAdjustment_Batch",
                table: "RII_BUDGET_PlanFishBatchAdjustment",
                columns: new[] { "BudgetPlanId", "BudgetPlanFishBatchId", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_RII_BUDGET_PlanFishBatchAdjustment_BudgetPlanFishBatchId",
                table: "RII_BUDGET_PlanFishBatchAdjustment",
                column: "BudgetPlanFishBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_RII_BUDGET_PlanFishBatchAdjustment_CreatedBy",
                table: "RII_BUDGET_PlanFishBatchAdjustment",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_RII_BUDGET_PlanFishBatchAdjustment_DeletedBy",
                table: "RII_BUDGET_PlanFishBatchAdjustment",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_RII_BUDGET_PlanFishBatchAdjustment_UpdatedBy",
                table: "RII_BUDGET_PlanFishBatchAdjustment",
                column: "UpdatedBy");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RII_BUDGET_PlanFishBatchAdjustment");
        }
    }
}
