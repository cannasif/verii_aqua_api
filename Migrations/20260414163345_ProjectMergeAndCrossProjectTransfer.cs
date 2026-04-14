using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace aqua_api.Migrations
{
    /// <inheritdoc />
    public partial class ProjectMergeAndCrossProjectTransfer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RII_ProjectMerge",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TargetProjectId = table.Column<long>(type: "bigint", nullable: false),
                    TargetProjectCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TargetProjectName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    MergeDate = table.Column<DateTime>(type: "date", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SourceProjectStateAfterMerge = table.Column<byte>(type: "tinyint", nullable: false),
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
                    table.PrimaryKey("PK_RII_ProjectMerge", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RII_ProjectMerge_RII_Project_TargetProjectId",
                        column: x => x.TargetProjectId,
                        principalTable: "RII_Project",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_ProjectMerge_RII_USERS_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "RII_USERS",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_ProjectMerge_RII_USERS_DeletedBy",
                        column: x => x.DeletedBy,
                        principalTable: "RII_USERS",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_ProjectMerge_RII_USERS_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "RII_USERS",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RII_ProjectMergeCage",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectMergeId = table.Column<long>(type: "bigint", nullable: false),
                    SourceProjectId = table.Column<long>(type: "bigint", nullable: false),
                    ProjectCageId = table.Column<long>(type: "bigint", nullable: false),
                    CageId = table.Column<long>(type: "bigint", nullable: false),
                    CageCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CageName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
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
                    table.PrimaryKey("PK_RII_ProjectMergeCage", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RII_ProjectMergeCage_RII_Cage_CageId",
                        column: x => x.CageId,
                        principalTable: "RII_Cage",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_ProjectMergeCage_RII_ProjectCage_ProjectCageId",
                        column: x => x.ProjectCageId,
                        principalTable: "RII_ProjectCage",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_ProjectMergeCage_RII_ProjectMerge_ProjectMergeId",
                        column: x => x.ProjectMergeId,
                        principalTable: "RII_ProjectMerge",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_ProjectMergeCage_RII_Project_SourceProjectId",
                        column: x => x.SourceProjectId,
                        principalTable: "RII_Project",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_ProjectMergeCage_RII_USERS_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "RII_USERS",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_ProjectMergeCage_RII_USERS_DeletedBy",
                        column: x => x.DeletedBy,
                        principalTable: "RII_USERS",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_ProjectMergeCage_RII_USERS_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "RII_USERS",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RII_ProjectMergeSource",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectMergeId = table.Column<long>(type: "bigint", nullable: false),
                    SourceProjectId = table.Column<long>(type: "bigint", nullable: false),
                    SourceProjectCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SourceProjectName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
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
                    table.PrimaryKey("PK_RII_ProjectMergeSource", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RII_ProjectMergeSource_RII_ProjectMerge_ProjectMergeId",
                        column: x => x.ProjectMergeId,
                        principalTable: "RII_ProjectMerge",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_ProjectMergeSource_RII_Project_SourceProjectId",
                        column: x => x.SourceProjectId,
                        principalTable: "RII_Project",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_ProjectMergeSource_RII_USERS_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "RII_USERS",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_ProjectMergeSource_RII_USERS_DeletedBy",
                        column: x => x.DeletedBy,
                        principalTable: "RII_USERS",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_ProjectMergeSource_RII_USERS_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "RII_USERS",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_RII_ProjectMerge_CreatedBy",
                table: "RII_ProjectMerge",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_RII_ProjectMerge_DeletedBy",
                table: "RII_ProjectMerge",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_RII_ProjectMerge_TargetProjectId",
                table: "RII_ProjectMerge",
                column: "TargetProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_RII_ProjectMerge_UpdatedBy",
                table: "RII_ProjectMerge",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_RII_ProjectMergeCage_CageId",
                table: "RII_ProjectMergeCage",
                column: "CageId");

            migrationBuilder.CreateIndex(
                name: "IX_RII_ProjectMergeCage_CreatedBy",
                table: "RII_ProjectMergeCage",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_RII_ProjectMergeCage_DeletedBy",
                table: "RII_ProjectMergeCage",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_RII_ProjectMergeCage_ProjectCageId",
                table: "RII_ProjectMergeCage",
                column: "ProjectCageId");

            migrationBuilder.CreateIndex(
                name: "IX_RII_ProjectMergeCage_ProjectMergeId",
                table: "RII_ProjectMergeCage",
                column: "ProjectMergeId");

            migrationBuilder.CreateIndex(
                name: "IX_RII_ProjectMergeCage_SourceProjectId",
                table: "RII_ProjectMergeCage",
                column: "SourceProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_RII_ProjectMergeCage_UpdatedBy",
                table: "RII_ProjectMergeCage",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_RII_ProjectMergeSource_CreatedBy",
                table: "RII_ProjectMergeSource",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_RII_ProjectMergeSource_DeletedBy",
                table: "RII_ProjectMergeSource",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_RII_ProjectMergeSource_ProjectMergeId",
                table: "RII_ProjectMergeSource",
                column: "ProjectMergeId");

            migrationBuilder.CreateIndex(
                name: "IX_RII_ProjectMergeSource_SourceProjectId",
                table: "RII_ProjectMergeSource",
                column: "SourceProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_RII_ProjectMergeSource_UpdatedBy",
                table: "RII_ProjectMergeSource",
                column: "UpdatedBy");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RII_ProjectMergeCage");

            migrationBuilder.DropTable(
                name: "RII_ProjectMergeSource");

            migrationBuilder.DropTable(
                name: "RII_ProjectMerge");
        }
    }
}
