using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace netpaymentswebserver.Migrations
{
    /// <inheritdoc />
    public partial class Removecollation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Surname2",
                schema: "main",
                table: "person",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                oldCollation: "no_accent");

            migrationBuilder.AlterColumn<string>(
                name: "Surname1",
                schema: "main",
                table: "person",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text",
                oldCollation: "no_accent");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                schema: "main",
                table: "person",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text",
                oldCollation: "no_accent");

            migrationBuilder.AlterColumn<string>(
                name: "DocumentId",
                schema: "main",
                table: "person",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldCollation: "no_accent");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                schema: "main",
                table: "group",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldCollation: "no_accent");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Surname2",
                schema: "main",
                table: "person",
                type: "text",
                nullable: true,
                collation: "no_accent",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Surname1",
                schema: "main",
                table: "person",
                type: "text",
                nullable: false,
                collation: "no_accent",
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                schema: "main",
                table: "person",
                type: "text",
                nullable: false,
                collation: "no_accent",
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "DocumentId",
                schema: "main",
                table: "person",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                collation: "no_accent",
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                schema: "main",
                table: "group",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                collation: "no_accent",
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);
        }
    }
}
