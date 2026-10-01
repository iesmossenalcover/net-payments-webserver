using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace netpaymentswebserver.Migrations
{
    /// <inheritdoc />
    public partial class EventTypeAndAuthorizations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "TransportAuthorization",
                schema: "main",
                table: "person_group_course",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "TransportAuthorizationDate",
                schema: "main",
                table: "person_group_course",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "WalkingAuthorization",
                schema: "main",
                table: "person_group_course",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "WalkingAuthorizationDate",
                schema: "main",
                table: "person_group_course",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Type",
                schema: "main",
                table: "event",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TransportAuthorization",
                schema: "main",
                table: "person_group_course");

            migrationBuilder.DropColumn(
                name: "TransportAuthorizationDate",
                schema: "main",
                table: "person_group_course");

            migrationBuilder.DropColumn(
                name: "WalkingAuthorization",
                schema: "main",
                table: "person_group_course");

            migrationBuilder.DropColumn(
                name: "WalkingAuthorizationDate",
                schema: "main",
                table: "person_group_course");

            migrationBuilder.DropColumn(
                name: "Type",
                schema: "main",
                table: "event");
        }
    }
}
