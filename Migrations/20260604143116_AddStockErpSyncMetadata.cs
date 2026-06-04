using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace aqua_api.Migrations
{
    /// <inheritdoc />
    public partial class AddStockErpSyncMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CountTriedBy",
                table: "RII_STOCK",
                type: "int",
                nullable: true,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ERPIntegrationNumber",
                table: "RII_STOCK",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsERPIntegrated",
                table: "RII_STOCK",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastSyncDate",
                table: "RII_STOCK",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CountTriedBy",
                table: "RII_STOCK");

            migrationBuilder.DropColumn(
                name: "ERPIntegrationNumber",
                table: "RII_STOCK");

            migrationBuilder.DropColumn(
                name: "IsERPIntegrated",
                table: "RII_STOCK");

            migrationBuilder.DropColumn(
                name: "LastSyncDate",
                table: "RII_STOCK");
        }
    }
}
