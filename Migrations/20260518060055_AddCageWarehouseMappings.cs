using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace aqua_api.Migrations
{
    /// <inheritdoc />
    public partial class AddCageWarehouseMappings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RII_CageWarehouseMapping",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CageId = table.Column<long>(type: "bigint", nullable: false),
                    WarehouseId = table.Column<long>(type: "bigint", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
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
                    table.PrimaryKey("PK_RII_CageWarehouseMapping", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RII_CageWarehouseMapping_RII_Cage_CageId",
                        column: x => x.CageId,
                        principalTable: "RII_Cage",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_CageWarehouseMapping_RII_USERS_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "RII_USERS",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_CageWarehouseMapping_RII_USERS_DeletedBy",
                        column: x => x.DeletedBy,
                        principalTable: "RII_USERS",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_CageWarehouseMapping_RII_USERS_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "RII_USERS",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_CageWarehouseMapping_RII_Warehouse_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "RII_Warehouse",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_RII_CageWarehouseMapping_CreatedBy",
                table: "RII_CageWarehouseMapping",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_RII_CageWarehouseMapping_DeletedBy",
                table: "RII_CageWarehouseMapping",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_RII_CageWarehouseMapping_UpdatedBy",
                table: "RII_CageWarehouseMapping",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_RII_CageWarehouseMapping_WarehouseId",
                table: "RII_CageWarehouseMapping",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "UX_RII_CageWarehouseMapping_Cage_Active",
                table: "RII_CageWarehouseMapping",
                column: "CageId",
                unique: true,
                filter: "[IsDeleted] = 0 AND [IsActive] = 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RII_CageWarehouseMapping");
        }
    }
}
