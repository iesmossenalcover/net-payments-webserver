using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace netpaymentswebserver.Migrations
{
    /// <inheritdoc />
    public partial class EventPublicationNotice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "NotifyExtracurricular",
                schema: "main",
                table: "event",
                type: "boolean",
                nullable: false,
                // Els events existents queden activats. No es posa HasDefaultValue al model: EF no enviaria
                // mai el false (és el valor per defecte del bool) i el desactivar en crear no tindria efecte.
                defaultValue: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "PublishNotifiedAt",
                schema: "main",
                table: "event",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UnpublishNotifiedAt",
                schema: "main",
                table: "event",
                type: "timestamp with time zone",
                nullable: true);

            // El que ja s'ha obert o tancat abans d'aquesta migració no s'ha d'avisar.
            migrationBuilder.Sql(@"UPDATE main.event SET ""PublishNotifiedAt"" = now() WHERE ""PublishDate"" <= now();");
            migrationBuilder.Sql(@"UPDATE main.event SET ""UnpublishNotifiedAt"" = now() WHERE ""UnpublishDate"" <= now();");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NotifyExtracurricular",
                schema: "main",
                table: "event");

            migrationBuilder.DropColumn(
                name: "PublishNotifiedAt",
                schema: "main",
                table: "event");

            migrationBuilder.DropColumn(
                name: "UnpublishNotifiedAt",
                schema: "main",
                table: "event");
        }
    }
}
