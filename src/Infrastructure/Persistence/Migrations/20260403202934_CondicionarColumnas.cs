using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Core.Manager.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CondicionarColumnas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "path_redirect",
                schema: "public",
                table: "modules",
                type: "text",
                nullable: false,
                defaultValue: "/dashboard",
                oldClrType: typeof(string),
                oldType: "text");

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

            migrationBuilder.AlterColumn<string>(
                name: "image_url",
                schema: "public",
                table: "modules",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "path_redirect",
                schema: "public",
                table: "modules",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text",
                oldDefaultValue: "/dashboard");

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

            migrationBuilder.AlterColumn<string>(
                name: "image_url",
                schema: "public",
                table: "modules",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }
    }
}
