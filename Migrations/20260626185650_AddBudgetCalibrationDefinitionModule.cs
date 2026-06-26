using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace aqua_api.Migrations
{
    /// <inheritdoc />
    public partial class AddBudgetCalibrationDefinitionModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RII_BUDGET_CALIBRATION_DEFINITION",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CalibrationCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CalibrationInfo = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
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
                    table.PrimaryKey("PK_RII_BUDGET_CALIBRATION_DEFINITION", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RII_BUDGET_CALIBRATION_DEFINITION_RII_USERS_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "RII_USERS",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_BUDGET_CALIBRATION_DEFINITION_RII_USERS_DeletedBy",
                        column: x => x.DeletedBy,
                        principalTable: "RII_USERS",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_BUDGET_CALIBRATION_DEFINITION_RII_USERS_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "RII_USERS",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_RII_BUDGET_CALIBRATION_DEFINITION_CreatedBy",
                table: "RII_BUDGET_CALIBRATION_DEFINITION",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_RII_BUDGET_CALIBRATION_DEFINITION_DeletedBy",
                table: "RII_BUDGET_CALIBRATION_DEFINITION",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_RII_BUDGET_CALIBRATION_DEFINITION_UpdatedBy",
                table: "RII_BUDGET_CALIBRATION_DEFINITION",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "UX_RII_BUDGET_CALIBRATION_DEFINITION_Code_Active",
                table: "RII_BUDGET_CALIBRATION_DEFINITION",
                column: "CalibrationCode",
                unique: true,
                filter: "[IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RII_BUDGET_CALIBRATION_DEFINITION");
        }
    }
}
