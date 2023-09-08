using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace netpaymentswebserver.Migrations
{
    /// <inheritdoc />
    public partial class NextSigninUO : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ChangePasswordNextSignIn",
                schema: "main",
                table: "uo_group_relation",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChangePasswordNextSignIn",
                schema: "main",
                table: "uo_group_relation");
        }
    }
}
