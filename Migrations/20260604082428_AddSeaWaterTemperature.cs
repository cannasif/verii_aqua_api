using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace aqua_api.Migrations
{
    /// <inheritdoc />
    public partial class AddSeaWaterTemperature : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RII_SEA_WATER_TEMPERATURE",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectId = table.Column<long>(type: "bigint", nullable: false),
                    ProjectCageId = table.Column<long>(type: "bigint", nullable: false),
                    RecordDate = table.Column<DateTime>(type: "date", nullable: false),
                    WaterTemperatureCelsius = table.Column<decimal>(type: "decimal(18,3)", nullable: true),
                    WeatherDescription = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_RII_SEA_WATER_TEMPERATURE", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RII_SEA_WATER_TEMPERATURE_RII_ProjectCage_ProjectCageId",
                        column: x => x.ProjectCageId,
                        principalTable: "RII_ProjectCage",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RII_SEA_WATER_TEMPERATURE_RII_Project_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "RII_Project",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RII_SEA_WATER_TEMPERATURE_RII_USERS_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "RII_USERS",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_SEA_WATER_TEMPERATURE_RII_USERS_DeletedBy",
                        column: x => x.DeletedBy,
                        principalTable: "RII_USERS",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_SEA_WATER_TEMPERATURE_RII_USERS_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "RII_USERS",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_RII_SEA_WATER_TEMPERATURE_CreatedBy",
                table: "RII_SEA_WATER_TEMPERATURE",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_RII_SEA_WATER_TEMPERATURE_DeletedBy",
                table: "RII_SEA_WATER_TEMPERATURE",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_RII_SEA_WATER_TEMPERATURE_ProjectCageId",
                table: "RII_SEA_WATER_TEMPERATURE",
                column: "ProjectCageId");

            migrationBuilder.CreateIndex(
                name: "IX_RII_SEA_WATER_TEMPERATURE_RecordDate",
                table: "RII_SEA_WATER_TEMPERATURE",
                column: "RecordDate");

            migrationBuilder.CreateIndex(
                name: "IX_RII_SEA_WATER_TEMPERATURE_UpdatedBy",
                table: "RII_SEA_WATER_TEMPERATURE",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "UX_RII_SEA_WATER_TEMPERATURE_ProjectCageDate_Active",
                table: "RII_SEA_WATER_TEMPERATURE",
                columns: new[] { "ProjectId", "ProjectCageId", "RecordDate" },
                unique: true,
                filter: "[IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RII_SEA_WATER_TEMPERATURE");
        }
    }
}
