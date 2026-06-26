using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace aqua_api.Migrations
{
    /// <inheritdoc />
    public partial class AddBudgetFishGrowthProfileModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RII_BUDGET_FISH_GROWTH_PROFILE",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StockId = table.Column<long>(type: "bigint", nullable: false),
                    StartMonth = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
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
                    table.PrimaryKey("PK_RII_BUDGET_FISH_GROWTH_PROFILE", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RII_BUDGET_FISH_GROWTH_PROFILE_RII_STOCK_StockId",
                        column: x => x.StockId,
                        principalTable: "RII_STOCK",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RII_BUDGET_FISH_GROWTH_PROFILE_RII_USERS_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "RII_USERS",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_BUDGET_FISH_GROWTH_PROFILE_RII_USERS_DeletedBy",
                        column: x => x.DeletedBy,
                        principalTable: "RII_USERS",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_BUDGET_FISH_GROWTH_PROFILE_RII_USERS_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "RII_USERS",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RII_BUDGET_FISH_GROWTH_PROFILE_LINE",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BudgetFishGrowthProfileId = table.Column<long>(type: "bigint", nullable: false),
                    GrowthMonthNo = table.Column<int>(type: "int", nullable: false),
                    CalendarMonth = table.Column<int>(type: "int", nullable: false),
                    MonthlyGrowthGram = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    TotalGram = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
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
                    table.PrimaryKey("PK_RII_BUDGET_FISH_GROWTH_PROFILE_LINE", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RII_BUDGET_FISH_GROWTH_PROFILE_LINE_RII_BUDGET_FISH_GROWTH_PROFILE_BudgetFishGrowthProfileId",
                        column: x => x.BudgetFishGrowthProfileId,
                        principalTable: "RII_BUDGET_FISH_GROWTH_PROFILE",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RII_BUDGET_FISH_GROWTH_PROFILE_LINE_RII_USERS_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "RII_USERS",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_BUDGET_FISH_GROWTH_PROFILE_LINE_RII_USERS_DeletedBy",
                        column: x => x.DeletedBy,
                        principalTable: "RII_USERS",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_BUDGET_FISH_GROWTH_PROFILE_LINE_RII_USERS_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "RII_USERS",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_RII_BUDGET_FISH_GROWTH_PROFILE_CreatedBy",
                table: "RII_BUDGET_FISH_GROWTH_PROFILE",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_RII_BUDGET_FISH_GROWTH_PROFILE_DeletedBy",
                table: "RII_BUDGET_FISH_GROWTH_PROFILE",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_RII_BUDGET_FISH_GROWTH_PROFILE_UpdatedBy",
                table: "RII_BUDGET_FISH_GROWTH_PROFILE",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "UX_RII_BUDGET_FISH_GROWTH_PROFILE_Stock_StartMonth_Active",
                table: "RII_BUDGET_FISH_GROWTH_PROFILE",
                columns: new[] { "StockId", "StartMonth" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_RII_BUDGET_FISH_GROWTH_PROFILE_LINE_CreatedBy",
                table: "RII_BUDGET_FISH_GROWTH_PROFILE_LINE",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_RII_BUDGET_FISH_GROWTH_PROFILE_LINE_DeletedBy",
                table: "RII_BUDGET_FISH_GROWTH_PROFILE_LINE",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_RII_BUDGET_FISH_GROWTH_PROFILE_LINE_UpdatedBy",
                table: "RII_BUDGET_FISH_GROWTH_PROFILE_LINE",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "UX_RII_BUDGET_FISH_GROWTH_PROFILE_LINE_Profile_Month_Active",
                table: "RII_BUDGET_FISH_GROWTH_PROFILE_LINE",
                columns: new[] { "BudgetFishGrowthProfileId", "GrowthMonthNo" },
                unique: true,
                filter: "[IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RII_BUDGET_FISH_GROWTH_PROFILE_LINE");

            migrationBuilder.DropTable(
                name: "RII_BUDGET_FISH_GROWTH_PROFILE");
        }
    }
}
