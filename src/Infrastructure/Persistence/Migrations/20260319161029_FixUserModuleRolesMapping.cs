using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Core.Manager.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixUserModuleRolesMapping : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "role_type",
                schema: "public",
                table: "roles",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "role_type");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "role_type",
                schema: "public",
                table: "roles",
                type: "role_type",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");
        }
    }
}
