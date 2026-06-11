using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace netpaymentswebserver.Migrations
{
    /// <inheritdoc />
    public partial class EventSyncGoogleCalendar : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CalendarEventId",
                schema: "main",
                table: "event",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "EndDate",
                schema: "main",
                table: "event",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CalendarEventId",
                schema: "main",
                table: "event");

            migrationBuilder.DropColumn(
                name: "EndDate",
                schema: "main",
                table: "event");
        }
    }
}
