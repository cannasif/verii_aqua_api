using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace aqua_api.Migrations
{
    /// <inheritdoc />
    public partial class AddBudgetPlanFishPrices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RII_BUDGET_PLAN_FISH_PRICE",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BudgetPlanId = table.Column<long>(type: "bigint", nullable: false),
                    CalibrationDefinitionId = table.Column<long>(type: "bigint", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    Month = table.Column<int>(type: "int", nullable: false),
                    UnitPriceEuro = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
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
                    table.PrimaryKey("PK_RII_BUDGET_PLAN_FISH_PRICE", x => x.Id);
                    table.CheckConstraint("CK_RII_BUDGET_PLAN_FISH_PRICE_MONTH", "[Month] BETWEEN 1 AND 12");
                    table.CheckConstraint("CK_RII_BUDGET_PLAN_FISH_PRICE_NON_NEGATIVE", "[UnitPriceEuro] >= 0");
                    table.ForeignKey(
                        name: "FK_RII_BUDGET_PLAN_FISH_PRICE_RII_BUDGET_CALIBRATION_DEFINITION_CalibrationDefinitionId",
                        column: x => x.CalibrationDefinitionId,
                        principalTable: "RII_BUDGET_CALIBRATION_DEFINITION",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_BUDGET_PLAN_FISH_PRICE_RII_BUDGET_PLAN_BudgetPlanId",
                        column: x => x.BudgetPlanId,
                        principalTable: "RII_BUDGET_PLAN",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RII_BUDGET_PLAN_FISH_PRICE_RII_USERS_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "RII_USERS",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_BUDGET_PLAN_FISH_PRICE_RII_USERS_DeletedBy",
                        column: x => x.DeletedBy,
                        principalTable: "RII_USERS",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_BUDGET_PLAN_FISH_PRICE_RII_USERS_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "RII_USERS",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_RII_BUDGET_PLAN_FISH_PRICE_CalibrationDefinitionId",
                table: "RII_BUDGET_PLAN_FISH_PRICE",
                column: "CalibrationDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_RII_BUDGET_PLAN_FISH_PRICE_CreatedBy",
                table: "RII_BUDGET_PLAN_FISH_PRICE",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_RII_BUDGET_PLAN_FISH_PRICE_DeletedBy",
                table: "RII_BUDGET_PLAN_FISH_PRICE",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_RII_BUDGET_PLAN_FISH_PRICE_UpdatedBy",
                table: "RII_BUDGET_PLAN_FISH_PRICE",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "UX_RII_BUDGET_PLAN_FISH_PRICE_PERIOD_ACTIVE",
                table: "RII_BUDGET_PLAN_FISH_PRICE",
                columns: new[] { "BudgetPlanId", "CalibrationDefinitionId", "Year", "Month" },
                unique: true,
                filter: "[IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RII_BUDGET_PLAN_FISH_PRICE");
        }
    }
}
