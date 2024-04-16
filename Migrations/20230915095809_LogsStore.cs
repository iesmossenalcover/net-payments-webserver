using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace netpaymentswebserver.Migrations
{
    /// <inheritdoc />
    public partial class LogsStore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_job_log_LogId",
                schema: "main",
                table: "job");

            migrationBuilder.DropPrimaryKey(
                name: "PK_log",
                schema: "main",
                table: "log");

            migrationBuilder.RenameTable(
                name: "log",
                schema: "main",
                newName: "log_info",
                newSchema: "main");

            migrationBuilder.AddPrimaryKey(
                name: "PK_log_info",
                schema: "main",
                table: "log_info",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_job_log_info_LogId",
                schema: "main",
                table: "job",
                column: "LogId",
                principalSchema: "main",
                principalTable: "log_info",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_job_log_info_LogId",
                schema: "main",
                table: "job");

            migrationBuilder.DropPrimaryKey(
                name: "PK_log_info",
                schema: "main",
                table: "log_info");

            migrationBuilder.RenameTable(
                name: "log_info",
                schema: "main",
                newName: "log",
                newSchema: "main");

            migrationBuilder.AddPrimaryKey(
                name: "PK_log",
                schema: "main",
                table: "log",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_job_log_LogId",
                schema: "main",
                table: "job",
                column: "LogId",
                principalSchema: "main",
                principalTable: "log",
                principalColumn: "Id");
        }
    }
}
