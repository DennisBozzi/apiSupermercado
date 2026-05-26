using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiSupermercado.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RenameUserNameField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Rename first_name → name (preserves data), then bump max length to 120.
            migrationBuilder.RenameColumn(
                name: "first_name",
                table: "users",
                newName: "name");

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "users",
                type: "character varying(120)",
                maxLength: 120,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(60)",
                oldMaxLength: 60,
                oldNullable: true);

            // last_name is gone now — single "name" field replaces both.
            migrationBuilder.DropColumn(
                name: "last_name",
                table: "users");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "last_name",
                table: "users",
                type: "character varying(60)",
                maxLength: 60,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "users",
                type: "character varying(60)",
                maxLength: 60,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(120)",
                oldMaxLength: 120,
                oldNullable: true);

            migrationBuilder.RenameColumn(
                name: "name",
                table: "users",
                newName: "first_name");
        }
    }
}
