using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace aqua_api.Migrations
{
    /// <inheritdoc />
    public partial class AddNetInventoryMovement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RII_NetInventoryMovement",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MovementNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NetType = table.Column<int>(type: "int", nullable: false),
                    MovementType = table.Column<int>(type: "int", nullable: false),
                    MovementDate = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    StockId = table.Column<long>(type: "bigint", nullable: true),
                    ProjectId = table.Column<long>(type: "bigint", nullable: true),
                    SourceWarehouseId = table.Column<long>(type: "bigint", nullable: true),
                    TargetWarehouseId = table.Column<long>(type: "bigint", nullable: true),
                    SourceProjectCageId = table.Column<long>(type: "bigint", nullable: true),
                    TargetProjectCageId = table.Column<long>(type: "bigint", nullable: true),
                    Quantity = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_RII_NetInventoryMovement", x => x.Id);
                    table.CheckConstraint("CK_RII_NetInventoryMovement_MovementType", "[MovementType] IN (1,2,3,4,5)");
                    table.CheckConstraint("CK_RII_NetInventoryMovement_NetType", "[NetType] IN (1,2)");
                    table.CheckConstraint("CK_RII_NetInventoryMovement_Quantity", "[Quantity] > 0");
                    table.ForeignKey(
                        name: "FK_RII_NetInventoryMovement_RII_ProjectCage_SourceProjectCageId",
                        column: x => x.SourceProjectCageId,
                        principalTable: "RII_ProjectCage",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_NetInventoryMovement_RII_ProjectCage_TargetProjectCageId",
                        column: x => x.TargetProjectCageId,
                        principalTable: "RII_ProjectCage",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_NetInventoryMovement_RII_Project_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "RII_Project",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_NetInventoryMovement_RII_STOCK_StockId",
                        column: x => x.StockId,
                        principalTable: "RII_STOCK",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_NetInventoryMovement_RII_USERS_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "RII_USERS",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_NetInventoryMovement_RII_USERS_DeletedBy",
                        column: x => x.DeletedBy,
                        principalTable: "RII_USERS",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_NetInventoryMovement_RII_USERS_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "RII_USERS",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_NetInventoryMovement_RII_Warehouse_SourceWarehouseId",
                        column: x => x.SourceWarehouseId,
                        principalTable: "RII_Warehouse",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_NetInventoryMovement_RII_Warehouse_TargetWarehouseId",
                        column: x => x.TargetWarehouseId,
                        principalTable: "RII_Warehouse",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_RII_NetInventoryMovement_CreatedBy",
                table: "RII_NetInventoryMovement",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_RII_NetInventoryMovement_DeletedBy",
                table: "RII_NetInventoryMovement",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_RII_NetInventoryMovement_Project_Date",
                table: "RII_NetInventoryMovement",
                columns: new[] { "ProjectId", "MovementDate" });

            migrationBuilder.CreateIndex(
                name: "IX_RII_NetInventoryMovement_SourceProjectCageId",
                table: "RII_NetInventoryMovement",
                column: "SourceProjectCageId");

            migrationBuilder.CreateIndex(
                name: "IX_RII_NetInventoryMovement_SourceWarehouseId",
                table: "RII_NetInventoryMovement",
                column: "SourceWarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_RII_NetInventoryMovement_StockId",
                table: "RII_NetInventoryMovement",
                column: "StockId");

            migrationBuilder.CreateIndex(
                name: "IX_RII_NetInventoryMovement_TargetCage_Date",
                table: "RII_NetInventoryMovement",
                columns: new[] { "TargetProjectCageId", "MovementDate" });

            migrationBuilder.CreateIndex(
                name: "IX_RII_NetInventoryMovement_TargetWarehouseId",
                table: "RII_NetInventoryMovement",
                column: "TargetWarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_RII_NetInventoryMovement_UpdatedBy",
                table: "RII_NetInventoryMovement",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "UX_RII_NetInventoryMovement_MovementNo_Active",
                table: "RII_NetInventoryMovement",
                column: "MovementNo",
                unique: true,
                filter: "[IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RII_NetInventoryMovement");
        }
    }
}
