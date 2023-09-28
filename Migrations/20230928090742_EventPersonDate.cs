using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace netpaymentswebserver.Migrations
{
    /// <inheritdoc />
    public partial class EventPersonDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DatePaid",
                schema: "main",
                table: "event_person",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DatePaid",
                schema: "main",
                table: "event_person");
        }
    }
}
