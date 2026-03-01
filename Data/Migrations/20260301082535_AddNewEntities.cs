using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VeilleNet.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddNewEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_JobExecutionLogs",
                table: "JobExecutionLogs");

            migrationBuilder.RenameTable(
                name: "x_tracked_accounts",
                newName: "x_tracked_accounts",
                newSchema: "containsharp");

            migrationBuilder.RenameTable(
                name: "JobExecutionLogs",
                newName: "job_execution_logs",
                newSchema: "containsharp");

            migrationBuilder.RenameIndex(
                name: "IX_JobExecutionLogs_Status",
                schema: "containsharp",
                table: "job_execution_logs",
                newName: "IX_job_execution_logs_Status");

            migrationBuilder.RenameIndex(
                name: "IX_JobExecutionLogs_StartedAt",
                schema: "containsharp",
                table: "job_execution_logs",
                newName: "IX_job_execution_logs_StartedAt");

            migrationBuilder.RenameIndex(
                name: "IX_JobExecutionLogs_JobName",
                schema: "containsharp",
                table: "job_execution_logs",
                newName: "IX_job_execution_logs_JobName");

            migrationBuilder.AddPrimaryKey(
                name: "PK_job_execution_logs",
                schema: "containsharp",
                table: "job_execution_logs",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_job_execution_logs",
                schema: "containsharp",
                table: "job_execution_logs");

            migrationBuilder.RenameTable(
                name: "x_tracked_accounts",
                schema: "containsharp",
                newName: "x_tracked_accounts");

            migrationBuilder.RenameTable(
                name: "job_execution_logs",
                schema: "containsharp",
                newName: "JobExecutionLogs");

            migrationBuilder.RenameIndex(
                name: "IX_job_execution_logs_Status",
                table: "JobExecutionLogs",
                newName: "IX_JobExecutionLogs_Status");

            migrationBuilder.RenameIndex(
                name: "IX_job_execution_logs_StartedAt",
                table: "JobExecutionLogs",
                newName: "IX_JobExecutionLogs_StartedAt");

            migrationBuilder.RenameIndex(
                name: "IX_job_execution_logs_JobName",
                table: "JobExecutionLogs",
                newName: "IX_JobExecutionLogs_JobName");

            migrationBuilder.AddPrimaryKey(
                name: "PK_JobExecutionLogs",
                table: "JobExecutionLogs",
                column: "Id");
        }
    }
}
