using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace aqua_api.Migrations
{
    /// <inheritdoc />
    public partial class AddCurrentDirectionModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RII_CURRENT_DIRECTION",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
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
                    table.PrimaryKey("PK_RII_CURRENT_DIRECTION", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RII_CURRENT_DIRECTION_RII_USERS_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "RII_USERS",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_CURRENT_DIRECTION_RII_USERS_DeletedBy",
                        column: x => x.DeletedBy,
                        principalTable: "RII_USERS",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_CURRENT_DIRECTION_RII_USERS_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "RII_USERS",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RRII_CURRENT_DIRECTION_MATCHES",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectId = table.Column<long>(type: "bigint", nullable: false),
                    ProjectCageId = table.Column<long>(type: "bigint", nullable: false),
                    CurrentDirectionId = table.Column<long>(type: "bigint", nullable: false),
                    RecordDate = table.Column<DateTime>(type: "date", nullable: false),
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
                    table.PrimaryKey("PK_RRII_CURRENT_DIRECTION_MATCHES", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RRII_CURRENT_DIRECTION_MATCHES_RII_CURRENT_DIRECTION_CurrentDirectionId",
                        column: x => x.CurrentDirectionId,
                        principalTable: "RII_CURRENT_DIRECTION",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RRII_CURRENT_DIRECTION_MATCHES_RII_ProjectCage_ProjectCageId",
                        column: x => x.ProjectCageId,
                        principalTable: "RII_ProjectCage",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RRII_CURRENT_DIRECTION_MATCHES_RII_Project_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "RII_Project",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RRII_CURRENT_DIRECTION_MATCHES_RII_USERS_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "RII_USERS",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RRII_CURRENT_DIRECTION_MATCHES_RII_USERS_DeletedBy",
                        column: x => x.DeletedBy,
                        principalTable: "RII_USERS",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RRII_CURRENT_DIRECTION_MATCHES_RII_USERS_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "RII_USERS",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_RII_CURRENT_DIRECTION_CreatedBy",
                table: "RII_CURRENT_DIRECTION",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_RII_CURRENT_DIRECTION_DeletedBy",
                table: "RII_CURRENT_DIRECTION",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_RII_CURRENT_DIRECTION_UpdatedBy",
                table: "RII_CURRENT_DIRECTION",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "UX_RII_CURRENT_DIRECTION_Name_Active",
                table: "RII_CURRENT_DIRECTION",
                column: "Name",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_RRII_CURRENT_DIRECTION_MATCHES_CreatedBy",
                table: "RRII_CURRENT_DIRECTION_MATCHES",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_RRII_CURRENT_DIRECTION_MATCHES_CurrentDirectionId",
                table: "RRII_CURRENT_DIRECTION_MATCHES",
                column: "CurrentDirectionId");

            migrationBuilder.CreateIndex(
                name: "IX_RRII_CURRENT_DIRECTION_MATCHES_DeletedBy",
                table: "RRII_CURRENT_DIRECTION_MATCHES",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_RRII_CURRENT_DIRECTION_MATCHES_ProjectCageId",
                table: "RRII_CURRENT_DIRECTION_MATCHES",
                column: "ProjectCageId");

            migrationBuilder.CreateIndex(
                name: "IX_RRII_CURRENT_DIRECTION_MATCHES_RecordDate",
                table: "RRII_CURRENT_DIRECTION_MATCHES",
                column: "RecordDate");

            migrationBuilder.CreateIndex(
                name: "IX_RRII_CURRENT_DIRECTION_MATCHES_UpdatedBy",
                table: "RRII_CURRENT_DIRECTION_MATCHES",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "UX_RRII_CURRENT_DIRECTION_MATCHES_ProjectCageDate_Active",
                table: "RRII_CURRENT_DIRECTION_MATCHES",
                columns: new[] { "ProjectId", "ProjectCageId", "RecordDate" },
                unique: true,
                filter: "[IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RRII_CURRENT_DIRECTION_MATCHES");

            migrationBuilder.DropTable(
                name: "RII_CURRENT_DIRECTION");
        }
    }
}
