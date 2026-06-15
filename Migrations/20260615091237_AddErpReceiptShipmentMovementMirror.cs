using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace aqua_api.Migrations
{
    /// <inheritdoc />
    public partial class AddErpReceiptShipmentMovementMirror : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RII_ERP_RECEIPT_SHIPMENT_MOVEMENT",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SourceSystem = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    SourceMovementKey = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    MovementDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    DocumentNo = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    ErpWarehouseCode = table.Column<short>(type: "smallint", nullable: true),
                    ErpProjectCode = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    ErpStockCode = table.Column<string>(type: "nvarchar(35)", maxLength: 35, nullable: false),
                    ErpStockName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Quantity = table.Column<decimal>(type: "decimal(28,8)", nullable: false),
                    MovementKind = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: false),
                    InOutCode = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: false),
                    StockGroupCode = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: true),
                    OperationType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ProjectId = table.Column<long>(type: "bigint", nullable: true),
                    CageId = table.Column<long>(type: "bigint", nullable: true),
                    ProjectCageId = table.Column<long>(type: "bigint", nullable: true),
                    StockId = table.Column<long>(type: "bigint", nullable: true),
                    FishBatchId = table.Column<long>(type: "bigint", nullable: true),
                    GoodsReceiptId = table.Column<long>(type: "bigint", nullable: true),
                    GoodsReceiptLineId = table.Column<long>(type: "bigint", nullable: true),
                    ShipmentId = table.Column<long>(type: "bigint", nullable: true),
                    ShipmentLineId = table.Column<long>(type: "bigint", nullable: true),
                    BatchMovementId = table.Column<long>(type: "bigint", nullable: true),
                    IsMatched = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsProcessed = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    ProcessingAttemptCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    LastSyncedAt = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
                    MatchedAt = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true),
                    ProcessedAt = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true),
                    MatchError = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ProcessError = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
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
                    table.PrimaryKey("PK_RII_ERP_RECEIPT_SHIPMENT_MOVEMENT", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RII_ERP_RECEIPT_SHIPMENT_MOVEMENT_RII_BatchMovement_BatchMovementId",
                        column: x => x.BatchMovementId,
                        principalTable: "RII_BatchMovement",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_ERP_RECEIPT_SHIPMENT_MOVEMENT_RII_Cage_CageId",
                        column: x => x.CageId,
                        principalTable: "RII_Cage",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_ERP_RECEIPT_SHIPMENT_MOVEMENT_RII_FishBatch_FishBatchId",
                        column: x => x.FishBatchId,
                        principalTable: "RII_FishBatch",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_ERP_RECEIPT_SHIPMENT_MOVEMENT_RII_GoodsReceiptLine_GoodsReceiptLineId",
                        column: x => x.GoodsReceiptLineId,
                        principalTable: "RII_GoodsReceiptLine",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_ERP_RECEIPT_SHIPMENT_MOVEMENT_RII_GoodsReceipt_GoodsReceiptId",
                        column: x => x.GoodsReceiptId,
                        principalTable: "RII_GoodsReceipt",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_ERP_RECEIPT_SHIPMENT_MOVEMENT_RII_ProjectCage_ProjectCageId",
                        column: x => x.ProjectCageId,
                        principalTable: "RII_ProjectCage",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_ERP_RECEIPT_SHIPMENT_MOVEMENT_RII_Project_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "RII_Project",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_ERP_RECEIPT_SHIPMENT_MOVEMENT_RII_STOCK_StockId",
                        column: x => x.StockId,
                        principalTable: "RII_STOCK",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_ERP_RECEIPT_SHIPMENT_MOVEMENT_RII_ShipmentLine_ShipmentLineId",
                        column: x => x.ShipmentLineId,
                        principalTable: "RII_ShipmentLine",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_ERP_RECEIPT_SHIPMENT_MOVEMENT_RII_Shipment_ShipmentId",
                        column: x => x.ShipmentId,
                        principalTable: "RII_Shipment",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_ERP_RECEIPT_SHIPMENT_MOVEMENT_RII_USERS_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "RII_USERS",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_ERP_RECEIPT_SHIPMENT_MOVEMENT_RII_USERS_DeletedBy",
                        column: x => x.DeletedBy,
                        principalTable: "RII_USERS",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_ERP_RECEIPT_SHIPMENT_MOVEMENT_RII_USERS_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "RII_USERS",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_RII_ERP_RECEIPT_SHIPMENT_MOVEMENT_BatchMovementId",
                table: "RII_ERP_RECEIPT_SHIPMENT_MOVEMENT",
                column: "BatchMovementId");

            migrationBuilder.CreateIndex(
                name: "IX_RII_ERP_RECEIPT_SHIPMENT_MOVEMENT_CageId",
                table: "RII_ERP_RECEIPT_SHIPMENT_MOVEMENT",
                column: "CageId");

            migrationBuilder.CreateIndex(
                name: "IX_RII_ERP_RECEIPT_SHIPMENT_MOVEMENT_CreatedBy",
                table: "RII_ERP_RECEIPT_SHIPMENT_MOVEMENT",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_RII_ERP_RECEIPT_SHIPMENT_MOVEMENT_DeletedBy",
                table: "RII_ERP_RECEIPT_SHIPMENT_MOVEMENT",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_RII_ERP_RECEIPT_SHIPMENT_MOVEMENT_FishBatchId",
                table: "RII_ERP_RECEIPT_SHIPMENT_MOVEMENT",
                column: "FishBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_RII_ERP_RECEIPT_SHIPMENT_MOVEMENT_GoodsReceipt",
                table: "RII_ERP_RECEIPT_SHIPMENT_MOVEMENT",
                columns: new[] { "GoodsReceiptId", "GoodsReceiptLineId" });

            migrationBuilder.CreateIndex(
                name: "IX_RII_ERP_RECEIPT_SHIPMENT_MOVEMENT_GoodsReceiptLineId",
                table: "RII_ERP_RECEIPT_SHIPMENT_MOVEMENT",
                column: "GoodsReceiptLineId");

            migrationBuilder.CreateIndex(
                name: "IX_RII_ERP_RECEIPT_SHIPMENT_MOVEMENT_ProcessState",
                table: "RII_ERP_RECEIPT_SHIPMENT_MOVEMENT",
                columns: new[] { "IsMatched", "IsProcessed" });

            migrationBuilder.CreateIndex(
                name: "IX_RII_ERP_RECEIPT_SHIPMENT_MOVEMENT_ProjectCageId",
                table: "RII_ERP_RECEIPT_SHIPMENT_MOVEMENT",
                column: "ProjectCageId");

            migrationBuilder.CreateIndex(
                name: "IX_RII_ERP_RECEIPT_SHIPMENT_MOVEMENT_ProjectId",
                table: "RII_ERP_RECEIPT_SHIPMENT_MOVEMENT",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_RII_ERP_RECEIPT_SHIPMENT_MOVEMENT_ProjectWarehouseDate",
                table: "RII_ERP_RECEIPT_SHIPMENT_MOVEMENT",
                columns: new[] { "ErpProjectCode", "ErpWarehouseCode", "MovementDate" });

            migrationBuilder.CreateIndex(
                name: "IX_RII_ERP_RECEIPT_SHIPMENT_MOVEMENT_Shipment",
                table: "RII_ERP_RECEIPT_SHIPMENT_MOVEMENT",
                columns: new[] { "ShipmentId", "ShipmentLineId" });

            migrationBuilder.CreateIndex(
                name: "IX_RII_ERP_RECEIPT_SHIPMENT_MOVEMENT_ShipmentLineId",
                table: "RII_ERP_RECEIPT_SHIPMENT_MOVEMENT",
                column: "ShipmentLineId");

            migrationBuilder.CreateIndex(
                name: "IX_RII_ERP_RECEIPT_SHIPMENT_MOVEMENT_StockId",
                table: "RII_ERP_RECEIPT_SHIPMENT_MOVEMENT",
                column: "StockId");

            migrationBuilder.CreateIndex(
                name: "IX_RII_ERP_RECEIPT_SHIPMENT_MOVEMENT_UpdatedBy",
                table: "RII_ERP_RECEIPT_SHIPMENT_MOVEMENT",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "UX_RII_ERP_RECEIPT_SHIPMENT_MOVEMENT_SourceMovementKey",
                table: "RII_ERP_RECEIPT_SHIPMENT_MOVEMENT",
                column: "SourceMovementKey",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RII_ERP_RECEIPT_SHIPMENT_MOVEMENT");
        }
    }
}
