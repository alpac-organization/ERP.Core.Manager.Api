using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Core.Manager.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class DeleteColumnOfSessionEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "access_token",
                schema: "public",
                table: "sessions");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                schema: "public",
                table: "sessions",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                schema: "public",
                table: "sessions");

            migrationBuilder.AddColumn<string>(
                name: "access_token",
                schema: "public",
                table: "sessions",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
