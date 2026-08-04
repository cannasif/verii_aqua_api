using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace aqua_api.Migrations
{
    /// <inheritdoc />
    public partial class AddErpMovementCancellationAudit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CancellationReason",
                table: "RII_ERP_RECEIPT_SHIPMENT_MOVEMENT",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CancelledAt",
                table: "RII_ERP_RECEIPT_SHIPMENT_MOVEMENT",
                type: "datetime2(3)",
                precision: 3,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "CancelledBy",
                table: "RII_ERP_RECEIPT_SHIPMENT_MOVEMENT",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsCancelled",
                table: "RII_ERP_RECEIPT_SHIPMENT_MOVEMENT",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_RII_ERP_RECEIPT_SHIPMENT_MOVEMENT_CANCELLED_DOCUMENT",
                table: "RII_ERP_RECEIPT_SHIPMENT_MOVEMENT",
                columns: new[] { "IsCancelled", "DocumentNo" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RII_ERP_RECEIPT_SHIPMENT_MOVEMENT_CANCELLED_DOCUMENT",
                table: "RII_ERP_RECEIPT_SHIPMENT_MOVEMENT");

            migrationBuilder.DropColumn(
                name: "CancellationReason",
                table: "RII_ERP_RECEIPT_SHIPMENT_MOVEMENT");

            migrationBuilder.DropColumn(
                name: "CancelledAt",
                table: "RII_ERP_RECEIPT_SHIPMENT_MOVEMENT");

            migrationBuilder.DropColumn(
                name: "CancelledBy",
                table: "RII_ERP_RECEIPT_SHIPMENT_MOVEMENT");

            migrationBuilder.DropColumn(
                name: "IsCancelled",
                table: "RII_ERP_RECEIPT_SHIPMENT_MOVEMENT");
        }
    }
}
