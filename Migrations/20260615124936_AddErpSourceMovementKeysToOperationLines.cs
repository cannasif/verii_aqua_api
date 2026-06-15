using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace aqua_api.Migrations
{
    /// <inheritdoc />
    public partial class AddErpSourceMovementKeysToOperationLines : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ErpSourceMovementKey",
                table: "RII_ShipmentLine",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ErpSourceMovementKey",
                table: "RII_GoodsReceiptLine",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "UX_RII_ShipmentLine_ErpSourceMovementKey_Active",
                table: "RII_ShipmentLine",
                column: "ErpSourceMovementKey",
                unique: true,
                filter: "[IsDeleted] = 0 AND [ErpSourceMovementKey] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UX_RII_GoodsReceiptLine_ErpSourceMovementKey_Active",
                table: "RII_GoodsReceiptLine",
                column: "ErpSourceMovementKey",
                unique: true,
                filter: "[IsDeleted] = 0 AND [ErpSourceMovementKey] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_RII_ShipmentLine_ErpSourceMovementKey_Active",
                table: "RII_ShipmentLine");

            migrationBuilder.DropIndex(
                name: "UX_RII_GoodsReceiptLine_ErpSourceMovementKey_Active",
                table: "RII_GoodsReceiptLine");

            migrationBuilder.DropColumn(
                name: "ErpSourceMovementKey",
                table: "RII_ShipmentLine");

            migrationBuilder.DropColumn(
                name: "ErpSourceMovementKey",
                table: "RII_GoodsReceiptLine");
        }
    }
}
