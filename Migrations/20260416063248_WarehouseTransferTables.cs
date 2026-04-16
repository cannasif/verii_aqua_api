using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace aqua_api.Migrations
{
    /// <inheritdoc />
    public partial class WarehouseTransferTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RII_GoodsReceipt_WarehouseCode_BranchCode",
                table: "RII_GoodsReceipt");

            migrationBuilder.DropCheckConstraint(
                name: "CK_RII_BatchMovement_MovementType",
                table: "RII_BatchMovement");

            migrationBuilder.DropColumn(
                name: "TargetWarehouse",
                table: "RII_Shipment");

            migrationBuilder.DropColumn(
                name: "WarehouseBranchCode",
                table: "RII_GoodsReceipt");

            migrationBuilder.DropColumn(
                name: "WarehouseCode",
                table: "RII_GoodsReceipt");

            migrationBuilder.AddColumn<long>(
                name: "TargetWarehouseId",
                table: "RII_Shipment",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "WarehouseId",
                table: "RII_GoodsReceipt",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "FromWarehouseId",
                table: "RII_BatchMovement",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ToWarehouseId",
                table: "RII_BatchMovement",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "WarehouseId",
                table: "RII_BatchMovement",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "RII_BatchWarehouseBalance",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectId = table.Column<long>(type: "bigint", nullable: false),
                    FishBatchId = table.Column<long>(type: "bigint", nullable: false),
                    WarehouseId = table.Column<long>(type: "bigint", nullable: false),
                    LiveCount = table.Column<int>(type: "int", nullable: false),
                    AverageGram = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 3, nullable: false),
                    BiomassGram = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 3, nullable: false),
                    AsOfDate = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
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
                    table.PrimaryKey("PK_RII_BatchWarehouseBalance", x => x.Id);
                    table.CheckConstraint("CK_RII_BatchWarehouseBalance_NonNegative", "[LiveCount] >= 0 AND [AverageGram] >= 0 AND [BiomassGram] >= 0");
                    table.ForeignKey(
                        name: "FK_RII_BatchWarehouseBalance_RII_FishBatch_FishBatchId",
                        column: x => x.FishBatchId,
                        principalTable: "RII_FishBatch",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_BatchWarehouseBalance_RII_Project_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "RII_Project",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_BatchWarehouseBalance_RII_USERS_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "RII_USERS",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_BatchWarehouseBalance_RII_USERS_DeletedBy",
                        column: x => x.DeletedBy,
                        principalTable: "RII_USERS",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_BatchWarehouseBalance_RII_USERS_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "RII_USERS",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_BatchWarehouseBalance_RII_Warehouse_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "RII_Warehouse",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RII_CageWarehouseTransfer",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectId = table.Column<long>(type: "bigint", nullable: false),
                    TransferNo = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    TransferDate = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
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
                    table.PrimaryKey("PK_RII_CageWarehouseTransfer", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RII_CageWarehouseTransfer_RII_Project_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "RII_Project",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_CageWarehouseTransfer_RII_USERS_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "RII_USERS",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_CageWarehouseTransfer_RII_USERS_DeletedBy",
                        column: x => x.DeletedBy,
                        principalTable: "RII_USERS",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_CageWarehouseTransfer_RII_USERS_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "RII_USERS",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RII_WarehouseCageTransfer",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectId = table.Column<long>(type: "bigint", nullable: false),
                    TransferNo = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    TransferDate = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
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
                    table.PrimaryKey("PK_RII_WarehouseCageTransfer", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RII_WarehouseCageTransfer_RII_Project_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "RII_Project",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_WarehouseCageTransfer_RII_USERS_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "RII_USERS",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_WarehouseCageTransfer_RII_USERS_DeletedBy",
                        column: x => x.DeletedBy,
                        principalTable: "RII_USERS",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_WarehouseCageTransfer_RII_USERS_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "RII_USERS",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RII_WarehouseTransfer",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectId = table.Column<long>(type: "bigint", nullable: false),
                    TransferNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TransferDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
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
                    table.PrimaryKey("PK_RII_WarehouseTransfer", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RII_WarehouseTransfer_RII_Project_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "RII_Project",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_WarehouseTransfer_RII_USERS_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "RII_USERS",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_WarehouseTransfer_RII_USERS_DeletedBy",
                        column: x => x.DeletedBy,
                        principalTable: "RII_USERS",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_WarehouseTransfer_RII_USERS_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "RII_USERS",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RII_CageWarehouseTransferLine",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CageWarehouseTransferId = table.Column<long>(type: "bigint", nullable: false),
                    FishBatchId = table.Column<long>(type: "bigint", nullable: false),
                    FromProjectCageId = table.Column<long>(type: "bigint", nullable: false),
                    ToWarehouseId = table.Column<long>(type: "bigint", nullable: false),
                    FishCount = table.Column<int>(type: "int", nullable: false),
                    AverageGram = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 3, nullable: false),
                    BiomassGram = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 3, nullable: false),
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
                    table.PrimaryKey("PK_RII_CageWarehouseTransferLine", x => x.Id);
                    table.CheckConstraint("CK_RII_CageWarehouseTransferLine_Positive", "[FishCount] > 0 AND [AverageGram] > 0 AND [BiomassGram] > 0");
                    table.ForeignKey(
                        name: "FK_RII_CageWarehouseTransferLine_RII_CageWarehouseTransfer_CageWarehouseTransferId",
                        column: x => x.CageWarehouseTransferId,
                        principalTable: "RII_CageWarehouseTransfer",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_CageWarehouseTransferLine_RII_FishBatch_FishBatchId",
                        column: x => x.FishBatchId,
                        principalTable: "RII_FishBatch",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_CageWarehouseTransferLine_RII_ProjectCage_FromProjectCageId",
                        column: x => x.FromProjectCageId,
                        principalTable: "RII_ProjectCage",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_CageWarehouseTransferLine_RII_USERS_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "RII_USERS",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_CageWarehouseTransferLine_RII_USERS_DeletedBy",
                        column: x => x.DeletedBy,
                        principalTable: "RII_USERS",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_CageWarehouseTransferLine_RII_USERS_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "RII_USERS",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_CageWarehouseTransferLine_RII_Warehouse_ToWarehouseId",
                        column: x => x.ToWarehouseId,
                        principalTable: "RII_Warehouse",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RII_WarehouseCageTransferLine",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WarehouseCageTransferId = table.Column<long>(type: "bigint", nullable: false),
                    FishBatchId = table.Column<long>(type: "bigint", nullable: false),
                    FromWarehouseId = table.Column<long>(type: "bigint", nullable: false),
                    ToProjectCageId = table.Column<long>(type: "bigint", nullable: false),
                    FishCount = table.Column<int>(type: "int", nullable: false),
                    AverageGram = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 3, nullable: false),
                    BiomassGram = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 3, nullable: false),
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
                    table.PrimaryKey("PK_RII_WarehouseCageTransferLine", x => x.Id);
                    table.CheckConstraint("CK_RII_WarehouseCageTransferLine_Positive", "[FishCount] > 0 AND [AverageGram] > 0 AND [BiomassGram] > 0");
                    table.ForeignKey(
                        name: "FK_RII_WarehouseCageTransferLine_RII_FishBatch_FishBatchId",
                        column: x => x.FishBatchId,
                        principalTable: "RII_FishBatch",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_WarehouseCageTransferLine_RII_ProjectCage_ToProjectCageId",
                        column: x => x.ToProjectCageId,
                        principalTable: "RII_ProjectCage",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_WarehouseCageTransferLine_RII_USERS_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "RII_USERS",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_WarehouseCageTransferLine_RII_USERS_DeletedBy",
                        column: x => x.DeletedBy,
                        principalTable: "RII_USERS",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_WarehouseCageTransferLine_RII_USERS_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "RII_USERS",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_WarehouseCageTransferLine_RII_WarehouseCageTransfer_WarehouseCageTransferId",
                        column: x => x.WarehouseCageTransferId,
                        principalTable: "RII_WarehouseCageTransfer",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_WarehouseCageTransferLine_RII_Warehouse_FromWarehouseId",
                        column: x => x.FromWarehouseId,
                        principalTable: "RII_Warehouse",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RII_WarehouseTransferLine",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WarehouseTransferId = table.Column<long>(type: "bigint", nullable: false),
                    FishBatchId = table.Column<long>(type: "bigint", nullable: false),
                    FromWarehouseId = table.Column<long>(type: "bigint", nullable: false),
                    ToWarehouseId = table.Column<long>(type: "bigint", nullable: false),
                    FishCount = table.Column<int>(type: "int", nullable: false),
                    AverageGram = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 3, nullable: false),
                    BiomassGram = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 3, nullable: false),
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
                    table.PrimaryKey("PK_RII_WarehouseTransferLine", x => x.Id);
                    table.CheckConstraint("CK_RII_WarehouseTransferLine_FromToDiff", "[FromWarehouseId] <> [ToWarehouseId]");
                    table.CheckConstraint("CK_RII_WarehouseTransferLine_Positive", "[FishCount] > 0 AND [AverageGram] > 0 AND [BiomassGram] > 0");
                    table.ForeignKey(
                        name: "FK_RII_WarehouseTransferLine_RII_FishBatch_FishBatchId",
                        column: x => x.FishBatchId,
                        principalTable: "RII_FishBatch",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_WarehouseTransferLine_RII_USERS_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "RII_USERS",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_WarehouseTransferLine_RII_USERS_DeletedBy",
                        column: x => x.DeletedBy,
                        principalTable: "RII_USERS",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_WarehouseTransferLine_RII_USERS_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "RII_USERS",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_WarehouseTransferLine_RII_WarehouseTransfer_WarehouseTransferId",
                        column: x => x.WarehouseTransferId,
                        principalTable: "RII_WarehouseTransfer",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_RII_Shipment_TargetWarehouseId",
                table: "RII_Shipment",
                column: "TargetWarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_RII_GoodsReceipt_WarehouseId",
                table: "RII_GoodsReceipt",
                column: "WarehouseId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_RII_BatchMovement_MovementType",
                table: "RII_BatchMovement",
                sql: "[MovementType] IN (0,1,2,3,4,5,6,7,8)");

            migrationBuilder.CreateIndex(
                name: "IX_RII_BatchWarehouseBalance_CreatedBy",
                table: "RII_BatchWarehouseBalance",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_RII_BatchWarehouseBalance_DeletedBy",
                table: "RII_BatchWarehouseBalance",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_RII_BatchWarehouseBalance_FishBatchId",
                table: "RII_BatchWarehouseBalance",
                column: "FishBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_RII_BatchWarehouseBalance_UpdatedBy",
                table: "RII_BatchWarehouseBalance",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_RII_BatchWarehouseBalance_WarehouseId",
                table: "RII_BatchWarehouseBalance",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "UX_RII_BatchWarehouseBalance_ProjectBatchWarehouse_Active",
                table: "RII_BatchWarehouseBalance",
                columns: new[] { "ProjectId", "FishBatchId", "WarehouseId" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_RII_CageWarehouseTransfer_CreatedBy",
                table: "RII_CageWarehouseTransfer",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_RII_CageWarehouseTransfer_DeletedBy",
                table: "RII_CageWarehouseTransfer",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_RII_CageWarehouseTransfer_ProjectId",
                table: "RII_CageWarehouseTransfer",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_RII_CageWarehouseTransfer_TransferNo",
                table: "RII_CageWarehouseTransfer",
                column: "TransferNo");

            migrationBuilder.CreateIndex(
                name: "IX_RII_CageWarehouseTransfer_UpdatedBy",
                table: "RII_CageWarehouseTransfer",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_RII_CageWarehouseTransferLine_CageWarehouseTransferId",
                table: "RII_CageWarehouseTransferLine",
                column: "CageWarehouseTransferId");

            migrationBuilder.CreateIndex(
                name: "IX_RII_CageWarehouseTransferLine_CreatedBy",
                table: "RII_CageWarehouseTransferLine",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_RII_CageWarehouseTransferLine_DeletedBy",
                table: "RII_CageWarehouseTransferLine",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_RII_CageWarehouseTransferLine_FishBatchId",
                table: "RII_CageWarehouseTransferLine",
                column: "FishBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_RII_CageWarehouseTransferLine_FromProjectCageId",
                table: "RII_CageWarehouseTransferLine",
                column: "FromProjectCageId");

            migrationBuilder.CreateIndex(
                name: "IX_RII_CageWarehouseTransferLine_ToWarehouseId",
                table: "RII_CageWarehouseTransferLine",
                column: "ToWarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_RII_CageWarehouseTransferLine_UpdatedBy",
                table: "RII_CageWarehouseTransferLine",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_RII_WarehouseCageTransfer_CreatedBy",
                table: "RII_WarehouseCageTransfer",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_RII_WarehouseCageTransfer_DeletedBy",
                table: "RII_WarehouseCageTransfer",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_RII_WarehouseCageTransfer_ProjectId",
                table: "RII_WarehouseCageTransfer",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_RII_WarehouseCageTransfer_TransferNo",
                table: "RII_WarehouseCageTransfer",
                column: "TransferNo");

            migrationBuilder.CreateIndex(
                name: "IX_RII_WarehouseCageTransfer_UpdatedBy",
                table: "RII_WarehouseCageTransfer",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_RII_WarehouseCageTransferLine_CreatedBy",
                table: "RII_WarehouseCageTransferLine",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_RII_WarehouseCageTransferLine_DeletedBy",
                table: "RII_WarehouseCageTransferLine",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_RII_WarehouseCageTransferLine_FishBatchId",
                table: "RII_WarehouseCageTransferLine",
                column: "FishBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_RII_WarehouseCageTransferLine_FromWarehouseId",
                table: "RII_WarehouseCageTransferLine",
                column: "FromWarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_RII_WarehouseCageTransferLine_ToProjectCageId",
                table: "RII_WarehouseCageTransferLine",
                column: "ToProjectCageId");

            migrationBuilder.CreateIndex(
                name: "IX_RII_WarehouseCageTransferLine_UpdatedBy",
                table: "RII_WarehouseCageTransferLine",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_RII_WarehouseCageTransferLine_WarehouseCageTransferId",
                table: "RII_WarehouseCageTransferLine",
                column: "WarehouseCageTransferId");

            migrationBuilder.CreateIndex(
                name: "IX_RII_WarehouseTransfer_CreatedBy",
                table: "RII_WarehouseTransfer",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_RII_WarehouseTransfer_DeletedBy",
                table: "RII_WarehouseTransfer",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_RII_WarehouseTransfer_ProjectId",
                table: "RII_WarehouseTransfer",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_RII_WarehouseTransfer_UpdatedBy",
                table: "RII_WarehouseTransfer",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_RII_WarehouseTransferLine_CreatedBy",
                table: "RII_WarehouseTransferLine",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_RII_WarehouseTransferLine_DeletedBy",
                table: "RII_WarehouseTransferLine",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_RII_WarehouseTransferLine_FishBatchId",
                table: "RII_WarehouseTransferLine",
                column: "FishBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_RII_WarehouseTransferLine_UpdatedBy",
                table: "RII_WarehouseTransferLine",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_RII_WarehouseTransferLine_WarehouseTransferId",
                table: "RII_WarehouseTransferLine",
                column: "WarehouseTransferId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RII_BatchWarehouseBalance");

            migrationBuilder.DropTable(
                name: "RII_CageWarehouseTransferLine");

            migrationBuilder.DropTable(
                name: "RII_WarehouseCageTransferLine");

            migrationBuilder.DropTable(
                name: "RII_WarehouseTransferLine");

            migrationBuilder.DropTable(
                name: "RII_CageWarehouseTransfer");

            migrationBuilder.DropTable(
                name: "RII_WarehouseCageTransfer");

            migrationBuilder.DropTable(
                name: "RII_WarehouseTransfer");

            migrationBuilder.DropIndex(
                name: "IX_RII_Shipment_TargetWarehouseId",
                table: "RII_Shipment");

            migrationBuilder.DropIndex(
                name: "IX_RII_GoodsReceipt_WarehouseId",
                table: "RII_GoodsReceipt");

            migrationBuilder.DropCheckConstraint(
                name: "CK_RII_BatchMovement_MovementType",
                table: "RII_BatchMovement");

            migrationBuilder.DropColumn(
                name: "TargetWarehouseId",
                table: "RII_Shipment");

            migrationBuilder.DropColumn(
                name: "WarehouseId",
                table: "RII_GoodsReceipt");

            migrationBuilder.DropColumn(
                name: "FromWarehouseId",
                table: "RII_BatchMovement");

            migrationBuilder.DropColumn(
                name: "ToWarehouseId",
                table: "RII_BatchMovement");

            migrationBuilder.DropColumn(
                name: "WarehouseId",
                table: "RII_BatchMovement");

            migrationBuilder.AddColumn<string>(
                name: "TargetWarehouse",
                table: "RII_Shipment",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WarehouseBranchCode",
                table: "RII_GoodsReceipt",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<short>(
                name: "WarehouseCode",
                table: "RII_GoodsReceipt",
                type: "smallint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RII_GoodsReceipt_WarehouseCode_BranchCode",
                table: "RII_GoodsReceipt",
                columns: new[] { "WarehouseCode", "WarehouseBranchCode" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_RII_BatchMovement_MovementType",
                table: "RII_BatchMovement",
                sql: "[MovementType] IN (0,1,2,3,4,5,6,7)");
        }
    }
}
