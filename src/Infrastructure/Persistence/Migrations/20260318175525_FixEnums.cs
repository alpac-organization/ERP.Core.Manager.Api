using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Core.Manager.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixEnums : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "user_status",
                schema: "public",
                table: "users",
                newName: "UserStatus");

            migrationBuilder.AlterColumn<string>(
                name: "UserStatus",
                schema: "public",
                table: "users",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "user_status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UserStatus",
                schema: "public",
                table: "users",
                newName: "user_status");

            migrationBuilder.AlterColumn<int>(
                name: "user_status",
                schema: "public",
                table: "users",
                type: "user_status",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");
        }
    }
}
