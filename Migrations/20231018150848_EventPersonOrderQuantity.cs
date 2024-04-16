using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace netpaymentswebserver.Migrations
{
    /// <inheritdoc />
    public partial class EventPersonOrderQuantity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_event_CourseId",
                schema: "main",
                table: "event");

            migrationBuilder.AddColumn<long>(
                name: "Quantity",
                schema: "main",
                table: "event_person_order",
                type: "bigint",
                nullable: false,
                defaultValue: 1L);

            migrationBuilder.CreateIndex(
                name: "IX_event_CourseId",
                schema: "main",
                table: "event",
                column: "CourseId",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_event_CourseId_UnpublishDate",
                schema: "main",
                table: "event",
                columns: new[] { "CourseId", "UnpublishDate" },
                descending: new bool[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_event_CourseId",
                schema: "main",
                table: "event");

            migrationBuilder.DropIndex(
                name: "IX_event_CourseId_UnpublishDate",
                schema: "main",
                table: "event");

            migrationBuilder.DropColumn(
                name: "Quantity",
                schema: "main",
                table: "event_person_order");

            migrationBuilder.CreateIndex(
                name: "IX_event_CourseId",
                schema: "main",
                table: "event",
                column: "CourseId");
        }
    }
}
