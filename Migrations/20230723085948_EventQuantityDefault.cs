using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace netpaymentswebserver.Migrations
{
    /// <inheritdoc />
    public partial class EventQuantityDefault : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxQuanity",
                schema: "main",
                table: "event");

            migrationBuilder.AlterColumn<long>(
                name: "Quantity",
                schema: "main",
                table: "event_person",
                type: "bigint",
                nullable: false,
                defaultValue: 1L,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<long>(
                name: "MaxQuantity",
                schema: "main",
                table: "event",
                type: "bigint",
                nullable: false,
                defaultValue: 1L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxQuantity",
                schema: "main",
                table: "event");

            migrationBuilder.AlterColumn<long>(
                name: "Quantity",
                schema: "main",
                table: "event_person",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldDefaultValue: 1L);

            migrationBuilder.AddColumn<long>(
                name: "MaxQuanity",
                schema: "main",
                table: "event",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }
    }
}
