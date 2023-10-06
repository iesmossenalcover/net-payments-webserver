using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace netpaymentswebserver.Migrations
{
    /// <inheritdoc />
    public partial class EventPersonIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_event_person_PersonId",
                schema: "main",
                table: "event_person",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_event_person_PersonId_Paid",
                schema: "main",
                table: "event_person",
                columns: new[] { "PersonId", "Paid" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_event_person_PersonId",
                schema: "main",
                table: "event_person");

            migrationBuilder.DropIndex(
                name: "IX_event_person_PersonId_Paid",
                schema: "main",
                table: "event_person");
        }
    }
}
