using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace netpaymentswebserver.Migrations
{
    /// <inheritdoc />
    public partial class EventPersonOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_event_person_order_OrderId",
                schema: "main",
                table: "event_person");

            migrationBuilder.RenameColumn(
                name: "OrderId",
                schema: "main",
                table: "event_person",
                newName: "PaidOrderId");

            migrationBuilder.RenameIndex(
                name: "IX_event_person_PersonId_EventId_OrderId",
                schema: "main",
                table: "event_person",
                newName: "IX_event_person_PersonId_EventId_PaidOrderId");

            migrationBuilder.RenameIndex(
                name: "IX_event_person_OrderId",
                schema: "main",
                table: "event_person",
                newName: "IX_event_person_PaidOrderId");

            migrationBuilder.CreateTable(
                name: "event_person_order",
                schema: "main",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EventPersonId = table.Column<long>(type: "bigint", nullable: false),
                    OrderId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_event_person_order", x => x.Id);
                    table.ForeignKey(
                        name: "FK_event_person_order_event_person_EventPersonId",
                        column: x => x.EventPersonId,
                        principalSchema: "main",
                        principalTable: "event_person",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_event_person_order_order_OrderId",
                        column: x => x.OrderId,
                        principalSchema: "main",
                        principalTable: "order",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_event_person_order_EventPersonId",
                schema: "main",
                table: "event_person_order",
                column: "EventPersonId");

            migrationBuilder.CreateIndex(
                name: "IX_event_person_order_OrderId",
                schema: "main",
                table: "event_person_order",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_event_person_order_OrderId_EventPersonId",
                schema: "main",
                table: "event_person_order",
                columns: new[] { "OrderId", "EventPersonId" });

            migrationBuilder.AddForeignKey(
                name: "FK_event_person_order_PaidOrderId",
                schema: "main",
                table: "event_person",
                column: "PaidOrderId",
                principalSchema: "main",
                principalTable: "order",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_event_person_order_PaidOrderId",
                schema: "main",
                table: "event_person");

            migrationBuilder.DropTable(
                name: "event_person_order",
                schema: "main");

            migrationBuilder.RenameColumn(
                name: "PaidOrderId",
                schema: "main",
                table: "event_person",
                newName: "OrderId");

            migrationBuilder.RenameIndex(
                name: "IX_event_person_PersonId_EventId_PaidOrderId",
                schema: "main",
                table: "event_person",
                newName: "IX_event_person_PersonId_EventId_OrderId");

            migrationBuilder.RenameIndex(
                name: "IX_event_person_PaidOrderId",
                schema: "main",
                table: "event_person",
                newName: "IX_event_person_OrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_event_person_order_OrderId",
                schema: "main",
                table: "event_person",
                column: "OrderId",
                principalSchema: "main",
                principalTable: "order",
                principalColumn: "Id");
        }
    }
}
