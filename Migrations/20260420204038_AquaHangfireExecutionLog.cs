using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace aqua_api.Migrations
{
    /// <inheritdoc />
    public partial class AquaHangfireExecutionLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RII_JOB_EXECUTION_LOG",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JobId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RecurringJobId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    JobName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Queue = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FinishedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DurationMs = table.Column<int>(type: "int", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ExceptionType = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ExceptionMessage = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    RetryCount = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_RII_JOB_EXECUTION_LOG", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RII_JOB_EXECUTION_LOG_RII_USERS_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "RII_USERS",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_JOB_EXECUTION_LOG_RII_USERS_DeletedBy",
                        column: x => x.DeletedBy,
                        principalTable: "RII_USERS",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RII_JOB_EXECUTION_LOG_RII_USERS_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "RII_USERS",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_JobExecutionLog_FinishedAt",
                table: "RII_JOB_EXECUTION_LOG",
                column: "FinishedAt");

            migrationBuilder.CreateIndex(
                name: "IX_JobExecutionLog_JobId",
                table: "RII_JOB_EXECUTION_LOG",
                column: "JobId");

            migrationBuilder.CreateIndex(
                name: "IX_JobExecutionLog_JobName",
                table: "RII_JOB_EXECUTION_LOG",
                column: "JobName");

            migrationBuilder.CreateIndex(
                name: "IX_JobExecutionLog_RecurringJobId",
                table: "RII_JOB_EXECUTION_LOG",
                column: "RecurringJobId");

            migrationBuilder.CreateIndex(
                name: "IX_JobExecutionLog_Status",
                table: "RII_JOB_EXECUTION_LOG",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_RII_JOB_EXECUTION_LOG_CreatedBy",
                table: "RII_JOB_EXECUTION_LOG",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_RII_JOB_EXECUTION_LOG_DeletedBy",
                table: "RII_JOB_EXECUTION_LOG",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_RII_JOB_EXECUTION_LOG_UpdatedBy",
                table: "RII_JOB_EXECUTION_LOG",
                column: "UpdatedBy");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RII_JOB_EXECUTION_LOG");
        }
    }
}
