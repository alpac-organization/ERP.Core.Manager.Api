using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Core.Manager.Api.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AgregarNuevaMigracion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "module_name",
                schema: "public",
                table: "modules",
                type: "character varying(180)",
                maxLength: 180,
                nullable: false,
                defaultValue: "/dashboard",
                oldClrType: typeof(string),
                oldType: "character varying(180)",
                oldMaxLength: 180);

            migrationBuilder.AddColumn<string>(
                name: "path_redirect",
                schema: "public",
                table: "modules",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "path_redirect",
                schema: "public",
                table: "modules");

            migrationBuilder.AlterColumn<string>(
                name: "module_name",
                schema: "public",
                table: "modules",
                type: "character varying(180)",
                maxLength: 180,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(180)",
                oldMaxLength: 180,
                oldDefaultValue: "/dashboard");
        }
    }
}
