using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace netpaymentswebserver.Migrations
{
    /// <inheritdoc />
    public partial class AmipDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "AmipaDate",
                schema: "main",
                table: "person_group_course",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "EnrolledDate",
                schema: "main",
                table: "person_group_course",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PaidAsAmipa",
                schema: "main",
                table: "event_person",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AmipaDate",
                schema: "main",
                table: "person_group_course");

            migrationBuilder.DropColumn(
                name: "EnrolledDate",
                schema: "main",
                table: "person_group_course");

            migrationBuilder.DropColumn(
                name: "PaidAsAmipa",
                schema: "main",
                table: "event_person");
        }
    }
}
