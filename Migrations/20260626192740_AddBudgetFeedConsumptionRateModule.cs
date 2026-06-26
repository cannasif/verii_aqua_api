using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace aqua_api.Migrations
{
    /// <inheritdoc />
    public partial class AddBudgetFeedConsumptionRateModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RII_BUDGET_FEED_CONSUMPTION_RATE",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WaterTemperatureId = table.Column<long>(type: "bigint", nullable: false),
                    CalibrationDefinitionId = table.Column<long>(type: "bigint", nullable: false),
                    FeedStockId = table.Column<long>(type: "bigint", nullable: false),
                    FeedAmount = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
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
                    table.PrimaryKey("PK_RII_BUDGET_FEED_CONSUMPTION_RATE", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RII_BUDGET_FEED_CONSUMPTION_RATE_RII_BUDGET_CALIBRATION_DEFINITION_CalibrationDefinitionId",
                        column: x => x.CalibrationDefinitionId,
                        principalTable: "RII_BUDGET_CALIBRATION_DEFINITION",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RII_BUDGET_FEED_CONSUMPTION_RATE_RII_BUDGET_WATER_TEMPERATURE_WaterTemperatureId",
                        column: x => x.WaterTemperatureId,
                        principalTable: "RII_BUDGET_WATER_TEMPERATURE",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RII_BUDGET_FEED_CONSUMPTION_RATE_RII_STOCK_FeedStockId",
                        column: x => x.FeedStockId,
                        principalTable: "RII_STOCK",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RII_BUDGET_FEED_CONSUMPTION_RATE_RII_USERS_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "RII_USERS",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_BUDGET_FEED_CONSUMPTION_RATE_RII_USERS_DeletedBy",
                        column: x => x.DeletedBy,
                        principalTable: "RII_USERS",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_BUDGET_FEED_CONSUMPTION_RATE_RII_USERS_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "RII_USERS",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_RII_BUDGET_FEED_CONSUMPTION_RATE_CalibrationDefinitionId",
                table: "RII_BUDGET_FEED_CONSUMPTION_RATE",
                column: "CalibrationDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_RII_BUDGET_FEED_CONSUMPTION_RATE_CreatedBy",
                table: "RII_BUDGET_FEED_CONSUMPTION_RATE",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_RII_BUDGET_FEED_CONSUMPTION_RATE_DeletedBy",
                table: "RII_BUDGET_FEED_CONSUMPTION_RATE",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_RII_BUDGET_FEED_CONSUMPTION_RATE_FeedStockId",
                table: "RII_BUDGET_FEED_CONSUMPTION_RATE",
                column: "FeedStockId");

            migrationBuilder.CreateIndex(
                name: "IX_RII_BUDGET_FEED_CONSUMPTION_RATE_UpdatedBy",
                table: "RII_BUDGET_FEED_CONSUMPTION_RATE",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "UX_RII_BUDGET_FEED_CONSUMPTION_RATE_Combination_Active",
                table: "RII_BUDGET_FEED_CONSUMPTION_RATE",
                columns: new[] { "WaterTemperatureId", "CalibrationDefinitionId", "FeedStockId" },
                unique: true,
                filter: "[IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RII_BUDGET_FEED_CONSUMPTION_RATE");
        }
    }
}
