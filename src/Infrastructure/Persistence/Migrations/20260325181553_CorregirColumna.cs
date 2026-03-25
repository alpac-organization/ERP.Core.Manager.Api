using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Core.Manager.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CorregirColumna : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RegisteredBy",
                schema: "public",
                table: "collaborators",
                newName: "registered_by");

            migrationBuilder.AlterColumn<string>(
                name: "registered_by",
                schema: "public",
                table: "collaborators",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "registered_by",
                schema: "public",
                table: "collaborators",
                newName: "RegisteredBy");

            migrationBuilder.AlterColumn<string>(
                name: "RegisteredBy",
                schema: "public",
                table: "collaborators",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");
        }
    }
}
