using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace netpaymentswebserver.Migrations
{
    /// <inheritdoc />
    public partial class AddCollation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_person_Name",
                schema: "main",
                table: "person");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:CollationDefinition:no_accent", "und-u-ks-level1-kc-true',und-u-ks-level1-kc-true',icu,False");

            migrationBuilder.CreateIndex(
                name: "IX_person_Name",
                schema: "main",
                table: "person",
                column: "Name")
                .Annotation("Relational:Collation", new[] { "no_accent" });

            migrationBuilder.CreateIndex(
                name: "IX_person_Surname1",
                schema: "main",
                table: "person",
                column: "Surname1")
                .Annotation("Relational:Collation", new[] { "no_accent" });

            migrationBuilder.CreateIndex(
                name: "IX_person_Surname2",
                schema: "main",
                table: "person",
                column: "Surname2")
                .Annotation("Relational:Collation", new[] { "no_accent" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_person_Name",
                schema: "main",
                table: "person");

            migrationBuilder.DropIndex(
                name: "IX_person_Surname1",
                schema: "main",
                table: "person");

            migrationBuilder.DropIndex(
                name: "IX_person_Surname2",
                schema: "main",
                table: "person");

            migrationBuilder.AlterDatabase()
                .OldAnnotation("Npgsql:CollationDefinition:no_accent", "und-u-ks-level1-kc-true',und-u-ks-level1-kc-true',icu,False");

            migrationBuilder.CreateIndex(
                name: "IX_person_Name",
                schema: "main",
                table: "person",
                column: "Name");
        }
    }
}
