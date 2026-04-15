using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace aqua_api.Migrations
{
    /// <inheritdoc />
    public partial class AquaLinePricingFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CurrencyCode",
                table: "RII_ShipmentLine",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "ExchangeRate",
                table: "RII_ShipmentLine",
                type: "decimal(18,6)",
                precision: 18,
                scale: 6,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "LineAmount",
                table: "RII_ShipmentLine",
                type: "decimal(18,6)",
                precision: 18,
                scale: 6,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "LocalLineAmount",
                table: "RII_ShipmentLine",
                type: "decimal(18,6)",
                precision: 18,
                scale: 6,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "LocalUnitPrice",
                table: "RII_ShipmentLine",
                type: "decimal(18,6)",
                precision: 18,
                scale: 6,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "UnitPrice",
                table: "RII_ShipmentLine",
                type: "decimal(18,6)",
                precision: 18,
                scale: 6,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CurrencyCode",
                table: "RII_GoodsReceiptLine",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "ExchangeRate",
                table: "RII_GoodsReceiptLine",
                type: "decimal(18,6)",
                precision: 18,
                scale: 6,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "LineAmount",
                table: "RII_GoodsReceiptLine",
                type: "decimal(18,6)",
                precision: 18,
                scale: 6,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "LocalLineAmount",
                table: "RII_GoodsReceiptLine",
                type: "decimal(18,6)",
                precision: 18,
                scale: 6,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "LocalUnitPrice",
                table: "RII_GoodsReceiptLine",
                type: "decimal(18,6)",
                precision: 18,
                scale: 6,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "UnitPrice",
                table: "RII_GoodsReceiptLine",
                type: "decimal(18,6)",
                precision: 18,
                scale: 6,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FeedCostFallbackStrategy",
                table: "RII_AquaSetting",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrencyCode",
                table: "RII_ShipmentLine");

            migrationBuilder.DropColumn(
                name: "ExchangeRate",
                table: "RII_ShipmentLine");

            migrationBuilder.DropColumn(
                name: "LineAmount",
                table: "RII_ShipmentLine");

            migrationBuilder.DropColumn(
                name: "LocalLineAmount",
                table: "RII_ShipmentLine");

            migrationBuilder.DropColumn(
                name: "LocalUnitPrice",
                table: "RII_ShipmentLine");

            migrationBuilder.DropColumn(
                name: "UnitPrice",
                table: "RII_ShipmentLine");

            migrationBuilder.DropColumn(
                name: "CurrencyCode",
                table: "RII_GoodsReceiptLine");

            migrationBuilder.DropColumn(
                name: "ExchangeRate",
                table: "RII_GoodsReceiptLine");

            migrationBuilder.DropColumn(
                name: "LineAmount",
                table: "RII_GoodsReceiptLine");

            migrationBuilder.DropColumn(
                name: "LocalLineAmount",
                table: "RII_GoodsReceiptLine");

            migrationBuilder.DropColumn(
                name: "LocalUnitPrice",
                table: "RII_GoodsReceiptLine");

            migrationBuilder.DropColumn(
                name: "UnitPrice",
                table: "RII_GoodsReceiptLine");

            migrationBuilder.DropColumn(
                name: "FeedCostFallbackStrategy",
                table: "RII_AquaSetting");
        }
    }
}
