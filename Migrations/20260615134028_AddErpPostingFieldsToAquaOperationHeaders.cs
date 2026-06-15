using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace aqua_api.Migrations
{
    /// <inheritdoc />
    public partial class AddErpPostingFieldsToAquaOperationHeaders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CountTriedBy",
                table: "RII_Shipment",
                type: "int",
                nullable: true,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ERPErrorMessage",
                table: "RII_Shipment",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ERPIntegrationDate",
                table: "RII_Shipment",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ERPIntegrationStatus",
                table: "RII_Shipment",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ERPReferenceNumber",
                table: "RII_Shipment",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsERPIntegrated",
                table: "RII_Shipment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "CountTriedBy",
                table: "RII_Mortality",
                type: "int",
                nullable: true,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ERPErrorMessage",
                table: "RII_Mortality",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ERPIntegrationDate",
                table: "RII_Mortality",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ERPIntegrationStatus",
                table: "RII_Mortality",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ERPReferenceNumber",
                table: "RII_Mortality",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsERPIntegrated",
                table: "RII_Mortality",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "MortalityNo",
                table: "RII_Mortality",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CountTriedBy",
                table: "RII_Feeding",
                type: "int",
                nullable: true,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ERPErrorMessage",
                table: "RII_Feeding",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ERPIntegrationDate",
                table: "RII_Feeding",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ERPIntegrationStatus",
                table: "RII_Feeding",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ERPReferenceNumber",
                table: "RII_Feeding",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsERPIntegrated",
                table: "RII_Feeding",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Shipment_ERPIntegrationStatus",
                table: "RII_Shipment",
                column: "ERPIntegrationStatus");

            migrationBuilder.CreateIndex(
                name: "IX_Shipment_IsERPIntegrated",
                table: "RII_Shipment",
                column: "IsERPIntegrated");

            migrationBuilder.CreateIndex(
                name: "IX_Mortality_ERPIntegrationStatus",
                table: "RII_Mortality",
                column: "ERPIntegrationStatus");

            migrationBuilder.CreateIndex(
                name: "IX_Mortality_IsERPIntegrated",
                table: "RII_Mortality",
                column: "IsERPIntegrated");

            migrationBuilder.CreateIndex(
                name: "UX_RII_Mortality_MortalityNo_Active",
                table: "RII_Mortality",
                column: "MortalityNo",
                unique: true,
                filter: "[IsDeleted] = 0 AND [MortalityNo] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Feeding_ERPIntegrationStatus",
                table: "RII_Feeding",
                column: "ERPIntegrationStatus");

            migrationBuilder.CreateIndex(
                name: "IX_Feeding_IsERPIntegrated",
                table: "RII_Feeding",
                column: "IsERPIntegrated");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Shipment_ERPIntegrationStatus",
                table: "RII_Shipment");

            migrationBuilder.DropIndex(
                name: "IX_Shipment_IsERPIntegrated",
                table: "RII_Shipment");

            migrationBuilder.DropIndex(
                name: "IX_Mortality_ERPIntegrationStatus",
                table: "RII_Mortality");

            migrationBuilder.DropIndex(
                name: "IX_Mortality_IsERPIntegrated",
                table: "RII_Mortality");

            migrationBuilder.DropIndex(
                name: "UX_RII_Mortality_MortalityNo_Active",
                table: "RII_Mortality");

            migrationBuilder.DropIndex(
                name: "IX_Feeding_ERPIntegrationStatus",
                table: "RII_Feeding");

            migrationBuilder.DropIndex(
                name: "IX_Feeding_IsERPIntegrated",
                table: "RII_Feeding");

            migrationBuilder.DropColumn(
                name: "CountTriedBy",
                table: "RII_Shipment");

            migrationBuilder.DropColumn(
                name: "ERPErrorMessage",
                table: "RII_Shipment");

            migrationBuilder.DropColumn(
                name: "ERPIntegrationDate",
                table: "RII_Shipment");

            migrationBuilder.DropColumn(
                name: "ERPIntegrationStatus",
                table: "RII_Shipment");

            migrationBuilder.DropColumn(
                name: "ERPReferenceNumber",
                table: "RII_Shipment");

            migrationBuilder.DropColumn(
                name: "IsERPIntegrated",
                table: "RII_Shipment");

            migrationBuilder.DropColumn(
                name: "CountTriedBy",
                table: "RII_Mortality");

            migrationBuilder.DropColumn(
                name: "ERPErrorMessage",
                table: "RII_Mortality");

            migrationBuilder.DropColumn(
                name: "ERPIntegrationDate",
                table: "RII_Mortality");

            migrationBuilder.DropColumn(
                name: "ERPIntegrationStatus",
                table: "RII_Mortality");

            migrationBuilder.DropColumn(
                name: "ERPReferenceNumber",
                table: "RII_Mortality");

            migrationBuilder.DropColumn(
                name: "IsERPIntegrated",
                table: "RII_Mortality");

            migrationBuilder.DropColumn(
                name: "MortalityNo",
                table: "RII_Mortality");

            migrationBuilder.DropColumn(
                name: "CountTriedBy",
                table: "RII_Feeding");

            migrationBuilder.DropColumn(
                name: "ERPErrorMessage",
                table: "RII_Feeding");

            migrationBuilder.DropColumn(
                name: "ERPIntegrationDate",
                table: "RII_Feeding");

            migrationBuilder.DropColumn(
                name: "ERPIntegrationStatus",
                table: "RII_Feeding");

            migrationBuilder.DropColumn(
                name: "ERPReferenceNumber",
                table: "RII_Feeding");

            migrationBuilder.DropColumn(
                name: "IsERPIntegrated",
                table: "RII_Feeding");
        }
    }
}
