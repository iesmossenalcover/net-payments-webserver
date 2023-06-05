using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace netpaymentswebserver.Migrations
{
    /// <inheritdoc />
    public partial class ChangeLastName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_person_Surname2",
                schema: "main",
                table: "person");

            migrationBuilder.DropColumn(
                name: "Surname2",
                schema: "main",
                table: "person");

            migrationBuilder.RenameColumn(
                name: "Surname1",
                schema: "main",
                table: "person",
                newName: "LastName");

            migrationBuilder.RenameIndex(
                name: "IX_person_Surname1",
                schema: "main",
                table: "person",
                newName: "IX_person_LastName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LastName",
                schema: "main",
                table: "person",
                newName: "Surname1");

            migrationBuilder.RenameIndex(
                name: "IX_person_LastName",
                schema: "main",
                table: "person",
                newName: "IX_person_Surname1");

            migrationBuilder.AddColumn<string>(
                name: "Surname2",
                schema: "main",
                table: "person",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_person_Surname2",
                schema: "main",
                table: "person",
                column: "Surname2");
        }
    }
}
