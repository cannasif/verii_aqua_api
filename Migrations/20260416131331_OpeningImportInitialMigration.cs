using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace aqua_api.Migrations
{
    /// <inheritdoc />
    public partial class OpeningImportInitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_RII_BatchMovement_MovementType",
                table: "RII_BatchMovement");

            migrationBuilder.CreateTable(
                name: "RII_OpeningImportJob",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FileName = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: false),
                    SourceSystem = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    MappingsJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SummaryJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PreviewedAt = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true),
                    AppliedAt = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true),
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
                    table.PrimaryKey("PK_RII_OpeningImportJob", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RII_OpeningImportJob_RII_USERS_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "RII_USERS",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_OpeningImportJob_RII_USERS_DeletedBy",
                        column: x => x.DeletedBy,
                        principalTable: "RII_USERS",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_OpeningImportJob_RII_USERS_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "RII_USERS",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RII_OpeningImportRow",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OpeningImportJobId = table.Column<long>(type: "bigint", nullable: false),
                    SheetName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RowNumber = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    RawDataJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NormalizedDataJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MessagesJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                    table.PrimaryKey("PK_RII_OpeningImportRow", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RII_OpeningImportRow_RII_OpeningImportJob_OpeningImportJobId",
                        column: x => x.OpeningImportJobId,
                        principalTable: "RII_OpeningImportJob",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_OpeningImportRow_RII_USERS_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "RII_USERS",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_OpeningImportRow_RII_USERS_DeletedBy",
                        column: x => x.DeletedBy,
                        principalTable: "RII_USERS",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_OpeningImportRow_RII_USERS_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "RII_USERS",
                        principalColumn: "Id");
                });

            migrationBuilder.AddCheckConstraint(
                name: "CK_RII_BatchMovement_MovementType",
                table: "RII_BatchMovement",
                sql: "[MovementType] IN (0,1,2,3,4,5,6,7,8,9)");

            migrationBuilder.CreateIndex(
                name: "IX_RII_OpeningImportJob_CreatedBy",
                table: "RII_OpeningImportJob",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_RII_OpeningImportJob_DeletedBy",
                table: "RII_OpeningImportJob",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_RII_OpeningImportJob_UpdatedBy",
                table: "RII_OpeningImportJob",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_RII_OpeningImportRow_CreatedBy",
                table: "RII_OpeningImportRow",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_RII_OpeningImportRow_DeletedBy",
                table: "RII_OpeningImportRow",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_RII_OpeningImportRow_JobSheetRow",
                table: "RII_OpeningImportRow",
                columns: new[] { "OpeningImportJobId", "SheetName", "RowNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_RII_OpeningImportRow_UpdatedBy",
                table: "RII_OpeningImportRow",
                column: "UpdatedBy");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RII_OpeningImportRow");

            migrationBuilder.DropTable(
                name: "RII_OpeningImportJob");

            migrationBuilder.DropCheckConstraint(
                name: "CK_RII_BatchMovement_MovementType",
                table: "RII_BatchMovement");

            migrationBuilder.AddCheckConstraint(
                name: "CK_RII_BatchMovement_MovementType",
                table: "RII_BatchMovement",
                sql: "[MovementType] IN (0,1,2,3,4,5,6,7,8)");
        }
    }
}
